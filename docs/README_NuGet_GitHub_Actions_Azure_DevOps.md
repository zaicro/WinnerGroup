# Publicar un NuGet en GitHub Packages con GitHub Actions

## 1. Qué vas a implementar

Tu código ya está en GitHub. El siguiente paso es agregar metadatos NuGet al proyecto y un workflow en `.github/workflows/nuget.yml`.

Flujo propuesto:

1. Push a `main`, pull request contra `main` o ejecución manual: restaurar, compilar, ejecutar pruebas y generar un `.nupkg` de CI como artifact descargable.
2. Push de un tag `v1.2.3`: validar y empaquetar esa versión; después publicarla en GitHub Packages.
3. La publicación descarga el paquete del job anterior: no recompila un paquete diferente.

GitHub Actions no utiliza la clave `stages` de Azure Pipelines. Aquí las responsabilidades se expresan con dos jobs: `build_test_pack` y `publish`, conectados con `needs`.

**Alcance:** GitHub.com, una biblioteca .NET empaquetable y pruebas ejecutables por `dotnet test` con VSTest. El ejemplo principal contempla VB.NET Framework 4.8 con proyecto de formato SDK y runner Windows. La adaptación de proyectos VB antiguos se explica más adelante.

Esta guía prepara el workflow; no modifica tu repositorio ni publica paquetes por sí sola.

## 2. Diferenciar los tres productos

| Elemento | Función |
|---|---|
| Repositorio Git | Código fuente e historial |
| GitHub Actions | Ejecutar restauración, compilación, pruebas y publicación |
| GitHub Packages, registro NuGet | Almacenar versiones consumibles mediante NuGet |
| Artifact de Actions | Resultado descargable de una ejecución; tiene retención y no es un feed NuGet |
| GitHub Release | Publicación asociada a un tag; no equivale por sí sola a publicar en el registro NuGet |

No necesitas crear un «feed» con el mismo procedimiento de Azure Artifacts. Para NuGet de GitHub Packages, el endpoint se basa en el propietario:

```text
https://nuget.pkg.github.com/OWNER/index.json
```

`OWNER` es tu cuenta o la organización, no el nombre del repositorio. La asociación al repositorio se indica con `RepositoryUrl`.

Fuente: [registro NuGet de GitHub](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry).

## 3. Estructura que asume el ejemplo

| Ruta | Contenido |
|---|---|
| `MiSolucion.sln` | Solución que incluye biblioteca y pruebas |
| `src/MiLibreria/MiLibreria.vbproj` | Biblioteca que se empaqueta |
| `tests/MiLibreria.Tests/MiLibreria.Tests.vbproj` | Proyecto de pruebas real |
| `.github/workflows/nuget.yml` | Pipeline GitHub Actions |

Si tus pruebas están en C#, cambia su ruta por el `.csproj`. Las pruebas pueden estar en otro lenguaje; eso no cambia el paquete VB.

Si aún no tienes pruebas, crea pruebas de comportamiento relevantes antes de presentar esta configuración como un pipeline probado. El workflow exige que se ejecute al menos una prueba. Si decides omitir esa etapa, elimínala explícitamente y documenta que no hay pruebas automatizadas.

## 4. Preparar el proyecto NuGet

Para un `.vbproj` de formato SDK, integra estas propiedades en el proyecto existente, adaptando los valores. No reemplaces referencias ni configuración del proyecto por este fragmento:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <OutputType>Library</OutputType>
    <IsPackable>true</IsPackable>
    <GeneratePackageOnBuild>false</GeneratePackageOnBuild>
    <PackageId>MiEmpresa.MiLibreria</PackageId>
    <Version>0.0.0-local</Version>
    <Authors>MiEmpresa</Authors>
    <Description>Descripción concreta de lo que ofrece la biblioteca.</Description>
    <RepositoryType>git</RepositoryType>
    <RepositoryUrl>https://github.com/OWNER/REPOSITORIO</RepositoryUrl>
  </PropertyGroup>
</Project>
```

El workflow sobrescribe `Version`, `PackageVersion` y `RepositoryUrl` al empaquetar. Evita versiones adicionales contradictorias en Directory.Build.props o .targets.

No declares `PackageLicenseExpression=MIT` a menos que realmente publiques el paquete bajo esa licencia. Las licencias y la visibilidad del código son decisiones distintas de poder usar GitHub Packages.

El paquete contiene la biblioteca y metadatos de dependencias, no automáticamente toda la solución ni los proyectos de prueba. Si necesitas archivos nativos, contenido o configuración especial, comprueba su inclusión antes de distribuir.

Un paquete `net48` está dirigido a .NET Framework 4.8 compatible. El YAML no convierte automáticamente esa biblioteca en una biblioteca para cualquier .NET moderno.

## 5. Workflow completo

Crea `.github/workflows/nuget.yml`. Cambia las tres rutas de `env` y la rama `main` si tu repositorio usa otra.

Las versiones mayores de actions corresponden a las documentadas al preparar esta guía. Son referencias mutables; una vez validado el flujo, puedes fijarlas a SHA completos verificados y gestionar actualizaciones. Para un runner propio, revisa además los requisitos de versión del agente.

```yaml
name: NuGet CI y publicación

on:
  pull_request:
    branches: [main]
  push:
    branches: [main]
    tags: ['v*']
  workflow_dispatch:

permissions:
  contents: read

concurrency:
  group: nuget-${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: false

env:
  SOLUTION: MiSolucion.sln
  PACKAGE_PROJECT: src/MiLibreria/MiLibreria.vbproj
  TEST_PROJECT: tests/MiLibreria.Tests/MiLibreria.Tests.vbproj
  CONFIGURATION: Release

jobs:
  build_test_pack:
    runs-on: windows-2022
    defaults:
      run:
        shell: pwsh
    steps:
      - name: Obtener código
        uses: actions/checkout@v7
        with:
          fetch-depth: 0
          persist-credentials: false

      - name: Preparar SDK
        uses: actions/setup-dotnet@v6
        with:
          dotnet-version: '10.0.x'

      - name: Comprobar rutas
        run: |
          foreach ($path in @($env:SOLUTION, $env:PACKAGE_PROJECT, $env:TEST_PROJECT)) {
            if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
              throw "No existe la ruta configurada: $path"
            }
          }

      - name: Determinar versión
        env:
          CURRENT_REF: ${{ github.ref }}
          CURRENT_TAG: ${{ github.ref_name }}
          RUN_NUMBER: ${{ github.run_number }}
        run: |
          if ($env:CURRENT_REF.StartsWith('refs/tags/')) {
            if ($env:CURRENT_TAG -notmatch '^v(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$') {
              throw 'El tag debe ser vMAJOR.MINOR.PATCH, por ejemplo v1.2.3.'
            }
            git merge-base --is-ancestor HEAD origin/main
            if ($LASTEXITCODE -ne 0) {
              throw 'El commit del tag debe formar parte de main.'
            }
            $version = $env:CURRENT_TAG.Substring(1)
          } else {
            $version = "0.0.0-ci.$env:RUN_NUMBER"
          }
          "PACKAGE_VERSION=$version" >> $env:GITHUB_ENV

      - name: Restaurar dependencias
        run: |
          dotnet restore "$env:SOLUTION"
          if ($LASTEXITCODE -ne 0) { throw 'Falló restore.' }

      - name: Compilar
        run: |
          dotnet build "$env:SOLUTION" --configuration "$env:CONFIGURATION" --no-restore "-p:Version=$env:PACKAGE_VERSION" -p:ContinuousIntegrationBuild=true
          if ($LASTEXITCODE -ne 0) { throw 'Falló build.' }

      - name: Ejecutar pruebas
        run: |
          dotnet test "$env:TEST_PROJECT" --configuration "$env:CONFIGURATION" --no-build --no-restore --logger "trx;LogFileName=tests.trx" --results-directory TestResults
          if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas.' }
          $reports = @(Get-ChildItem TestResults -Recurse -Filter '*.trx')
          if ($reports.Count -eq 0) { throw 'No se generaron resultados TRX.' }
          $total = 0
          foreach ($report in $reports) {
            [xml]$trx = Get-Content -LiteralPath $report.FullName -Raw
            $total += [int]$trx.TestRun.ResultSummary.Counters.executed
          }
          if ($total -lt 1) { throw 'No se ejecutó ninguna prueba.' }

      - name: Conservar resultados de pruebas
        if: always()
        uses: actions/upload-artifact@v7
        with:
          name: test-results
          path: TestResults/**/*.trx
          if-no-files-found: warn
          retention-days: 7

      - name: Generar paquete NuGet
        run: |
          dotnet pack "$env:PACKAGE_PROJECT" --configuration "$env:CONFIGURATION" --no-build --no-restore --output artifacts "-p:Version=$env:PACKAGE_VERSION" "-p:PackageVersion=$env:PACKAGE_VERSION" "-p:RepositoryUrl=https://github.com/$env:GITHUB_REPOSITORY" -p:ContinuousIntegrationBuild=true
          if ($LASTEXITCODE -ne 0) { throw 'Falló pack.' }
          $packages = @(Get-ChildItem artifacts -Filter '*.nupkg')
          if ($packages.Count -ne 1) { throw 'Se esperaba exactamente un .nupkg.' }

      - name: Conservar paquete
        uses: actions/upload-artifact@v7
        with:
          name: nuget-package
          path: artifacts/*.nupkg
          if-no-files-found: error
          retention-days: 7

  publish:
    needs: build_test_pack
    if: github.event_name == 'push' && startsWith(github.ref, 'refs/tags/v')
    runs-on: windows-2022
    permissions:
      contents: read
      packages: write
    defaults:
      run:
        shell: pwsh
    steps:
      - name: Preparar SDK
        uses: actions/setup-dotnet@v6
        with:
          dotnet-version: '10.0.x'

      - name: Descargar paquete validado
        uses: actions/download-artifact@v8
        with:
          name: nuget-package
          path: artifacts

      - name: Publicar en GitHub Packages
        env:
          GH_PACKAGES_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          GH_PACKAGES_USER: ${{ github.actor }}
          GH_PACKAGES_OWNER: ${{ github.repository_owner }}
        run: |
          $source = "https://nuget.pkg.github.com/$env:GH_PACKAGES_OWNER/index.json"
          $config = Join-Path $env:RUNNER_TEMP 'nuget-publish.config'
          '<configuration><packageSources><clear /></packageSources></configuration>' | Set-Content -LiteralPath $config
          try {
            dotnet nuget add source "$source" --name github --username "$env:GH_PACKAGES_USER" --password "$env:GH_PACKAGES_TOKEN" --store-password-in-clear-text --configfile "$config"
            if ($LASTEXITCODE -ne 0) { throw 'Falló la autenticación NuGet.' }
            $packages = @(Get-ChildItem artifacts -Filter '*.nupkg')
            if ($packages.Count -ne 1) { throw 'Se esperaba exactamente un paquete.' }
            dotnet nuget push "$($packages[0].FullName)" --source github --api-key "$env:GH_PACKAGES_TOKEN" --configfile "$config"
            if ($LASTEXITCODE -ne 0) { throw 'Falló la publicación.' }
          } finally {
            Remove-Item -LiteralPath $config -Force -ErrorAction SilentlyContinue
          }
```

### Decisiones del workflow

- Usa Windows porque net48 y las pruebas .NET Framework lo requieren en este escenario.
- El SDK instalado compila proyectos SDK; no cambia el framework objetivo `net48`.
- Si ya tienes `global.json`, alinea el SDK instalado con él o utiliza `global-json-file` en setup-dotnet. No dejes fijado un SDK distinto que el runner no instale.
- Los proyectos de biblioteca y pruebas deben estar incluidos en la solución, con compilación Release habilitada; `pack` y `test` reutilizan ese build.
- Solo `publish` recibe `packages: write`. Un PR no publica ni necesita ese permiso.
- `GITHUB_TOKEN` lo genera GitHub para la ejecución: no tienes que crear un secret manual con ese nombre.
- La configuración con el token en claro se crea solo en la carpeta temporal del runner de publicación y se elimina. No se versiona ni se sube como artifact.
- No usa `--skip-duplicate`: publicar otra vez la misma versión debe mostrar el conflicto, no ocultarlo. Si necesitas reanudar después de un push que sí terminó, comprueba primero qué versión quedó publicada.
- El tag se valida y debe apuntar a un commit perteneciente a `main`. Cambia `origin/main` también si renombraste la rama.
- Las versiones de CI no se publican al feed. Este ejemplo acepta releases estables; tags como `v1.2.3-beta.1` requieren ampliar la validación conscientemente.

No se incluye firma del paquete ni generación de `.snupkg`. Puedes incorporarlas cuando exista un requisito; no son condiciones universales para publicar tu primer paquete.

## 6. Permisos que debes revisar en GitHub

1. Verifica que Actions esté habilitado y que la organización permita las actions utilizadas.
2. Revisa Settings → Actions → General → Workflow permissions y las políticas de la organización: el job de publicación debe poder recibir `packages: write`.
3. En un paquete existente, verifica que el repositorio tenga acceso de escritura al paquete. `packages: write` no concede acceso a cualquier paquete de otra cuenta o repositorio.
4. `RepositoryUrl` y el endpoint deben corresponder al mismo propietario. El workflow usa el repositorio actual para evitar discrepancias.
5. Después de publicar, comprueba visibilidad y permisos en la configuración del paquete. No asumas que será público porque el repositorio lo sea.

Fuente: [publicación con GitHub Actions y permisos](https://docs.github.com/en/packages/managing-github-packages-using-github-actions-workflows/publishing-and-installing-a-package-with-github-actions).

Si la biblioteca depende de otros paquetes privados, el restore del primer job necesita credenciales de lectura y acceso a esos paquetes. El ejemplo supone dependencias públicas o fuentes ya accesibles; no añade un PAT de publicación al job de PR. Los PR de forks no deben depender de secretos que GitHub no les entrega.

## 7. Primera publicación

1. Adapta PackageId, metadatos, rutas, rama y SDK.
2. Confirma que restaurar, compilar, probar y empaquetar funcionan localmente.
3. Sube el YAML y revisa el resultado de CI en Actions.
4. Integra el commit en `main`.
5. Desde ese commit, crea y sube un tag:

```powershell
git switch main
git pull --ff-only
git tag -a v1.0.0 -m "Publicar MiEmpresa.MiLibreria 1.0.0"
git push origin v1.0.0
```

6. En Actions, comprueba ambos jobs. En Packages del propietario/repositorio, verifica el paquete y su versión.
7. Instálalo en un proyecto consumidor de prueba antes de anunciarlo como disponible.

Para la siguiente versión utiliza un tag nuevo: `v1.0.1`, `v1.1.0` o `v2.0.0`, según los cambios. No muevas tags publicados para intentar sustituir una versión existente.

## 8. Consumir el paquete en otro proyecto

Publicar no configura automáticamente Visual Studio en los equipos consumidores.

Agrega un `nuget.config` sin credenciales al repositorio consumidor:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/OWNER/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="github">
      <package pattern="MiEmpresa.*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

Cambia OWNER y el prefijo. El mapeo de prefijo más específico dirige tus paquetes a GitHub y las demás dependencias a nuget.org. Todos los paquetes que coincidan con ese prefijo deben estar disponibles en la fuente elegida.

Para consumo local, crea un **PAT classic** con `read:packages`, acceso al paquete y autorización SSO si aplica. No presupongas que un fine-grained PAT sirve para el registro. GitHub exige autenticación también para consumir paquetes NuGet públicos.

En una sesión PowerShell local, sin escribir el token literal en el comando ni en el repositorio:

```powershell
$credential = Get-Credential -Message 'Usuario GitHub y PAT classic como contraseña'
$pat = $credential.GetNetworkCredential().Password
$env:NuGetPackageSourceCredentials_github = "Username=$($credential.UserName);Password=$pat;ValidAuthenticationTypes=Basic"
try {
    dotnet add .\MiConsumidor.vbproj package MiEmpresa.MiLibreria --version 1.0.0
    if ($LASTEXITCODE -ne 0) { throw 'Falló la instalación del paquete.' }
    dotnet restore .\MiConsumidor.vbproj
    if ($LASTEXITCODE -ne 0) { throw 'Falló restore.' }
} finally {
    Remove-Item Env:\NuGetPackageSourceCredentials_github -ErrorAction SilentlyContinue
    $pat = $null
    $credential = $null
}
```

Esto proporciona credenciales al proceso CLI de esa sesión; no configura permanentemente el gestor NuGet de un Visual Studio abierto aparte. Para Visual Studio, configura la fuente y su autenticación mediante las opciones del cliente o un mecanismo de credenciales aprobado.

Otro repositorio puede utilizar su `GITHUB_TOKEN` si tiene acceso al paquete y permiso `packages: read`; si no, necesita un secreto de lectura adecuado. La identidad que consume necesita acceso aunque la que publicó sí lo tuviera.

## 9. Si tu VB.NET 4.8 usa el proyecto clásico

Que un proyecto sea VB.NET 4.8 no implica que sea SDK-style. Si el `.vbproj` tiene imports tradicionales de Visual Basic, referencias con HintPath o packages.config, prueba primero su cadena de herramientas.

| Proyecto | Ruta de empaquetado |
|---|---|
| SDK-style | Workflow anterior con dotnet restore/build/test/pack |
| VB clásico no compatible con dotnet pack | NuGet restore + MSBuild de Visual Studio + ejecución de tests compatible + nuget pack con `.nuspec` |

En la segunda ruta el runner Windows necesita preparar MSBuild/NuGet y un `.nuspec` que declare archivos y dependencias correctamente. No basta con añadir `PackageId` a un proyecto antiguo y asumir que funcionará. Mantén la etapa final que publica el `.nupkg`; adapta cómo lo construyes y pruebas.

Para proyectos clásicos, una alternativa es un proyecto SDK de empaquetado separado, siempre que declares explícitamente qué binarios y dependencias incluye. No cambies el formato de una aplicación legacy solo para publicar un paquete sin evaluar sus imports y dependencias.

## 10. Coste con una cuenta GitHub Free

Puedes usar GitHub Packages y GitHub Actions sin contratar automáticamente un plan de pago. Los paquetes públicos tienen uso gratuito de Packages; los privados consumen las cuotas del propietario. La documentación consultada indica 500 MB de almacenamiento Packages para GitHub Free. Actions tiene su propia medición y límites de uso; no confundas ambas cuotas.

Revisa los límites y presupuestos actuales de la cuenta antes de generar muchas versiones o artifacts. El ejemplo usa retención de 7 días para artifacts de CI; esa retención no elimina las versiones del feed NuGet.

Fuentes: [facturación Packages](https://docs.github.com/en/billing/concepts/product-billing/github-packages), [uso incluido](https://docs.github.com/en/billing/reference/product-usage-included), [facturación Actions](https://docs.github.com/en/billing/concepts/product-billing/github-actions).

## 11. ¿El YAML de GitHub sirve en Azure DevOps y viceversa?

**No se ejecuta directamente.** YAML es el formato de texto; GitHub Actions y Azure Pipelines tienen esquemas, expresiones y tareas distintos.

| Concepto | GitHub Actions | Azure Pipelines |
|---|---|---|
| Archivo habitual | `.github/workflows/nuget.yml` | `azure-pipelines.yml` |
| Disparadores | `on` | `trigger`, `pr` y configuración del repositorio |
| Agente | `runs-on: windows-2022` | `pool: { vmImage: windows-2022 }` |
| Instalación del SDK | `actions/setup-dotnet` | Tarea `UseDotNet@2` |
| Ejecución | `run`, `shell: pwsh` | `pwsh`, `script` o tareas |
| Dependencias entre jobs | `needs` | `dependsOn` |
| Fases | Jobs/workflows y environments según el flujo | `stages` |
| Variable de entorno en PowerShell | `$env:NOMBRE` | `$env:NOMBRE` |
| Expresión del pipeline | `${{ github.ref }}` | Expresiones Azure como `variables[...]` según su contexto |
| Token automático | `GITHUB_TOKEN` | `System.AccessToken`, válido en Azure según permisos |
| Artefacto entre jobs | upload-artifact / download-artifact | PublishPipelineArtifact / DownloadPipelineArtifact |
| Registro de paquetes | GitHub Packages | Azure Artifacts, u otro registro configurado |

Un token Azure no autentica automáticamente en GitHub Packages, ni un GITHUB_TOKEN sustituye una identidad Azure Artifacts. Cambiar de motor de pipeline y cambiar de registro NuGet son decisiones independientes.

Ejemplo mínimo del mismo comando:

```yaml
# GitHub Actions: fragmento de un job
steps:
  - name: Compilar
    shell: pwsh
    run: dotnet build MiSolucion.sln --configuration Release
```

```yaml
# Azure Pipelines: fragmento de un job
steps:
  - pwsh: dotnet build MiSolucion.sln --configuration Release
    displayName: Compilar
```

Lo reutilizable son los comandos `dotnet`, `.nuspec`, scripts PowerShell y la lógica de empaquetado. Mantén esa lógica en scripts compartidos y deja que cada YAML configure credenciales, agentes, eventos y dependencias.

También puedes ejecutar Azure Pipelines sobre un repositorio alojado en GitHub. Eso no convierte el YAML de GitHub Actions: Azure ejecuta su propia definición.

Fuentes: [migración de Azure Pipelines a Actions](https://docs.github.com/en/actions/tutorials/migrate-to-github-actions/manual-migrations/migrate-from-azure-pipelines), [Azure Pipelines con repositorios GitHub](https://learn.microsoft.com/en-us/azure/devops/pipelines/repos/github?view=azure-devops).

## 12. Errores frecuentes

| Síntoma | Comprobación |
|---|---|
| 401 al restaurar | PAT classic/credencial ausente, vencida o no disponible para ese cliente |
| 403 al publicar | packages: write, acceso al paquete, política de organización y OWNER correctos |
| 409 o versión duplicada | Publica una versión nueva; comprueba si una ejecución anterior ya terminó |
| El paquete no aparece asociado al repo | Verifica RepositoryUrl y el propietario del registro |
| CI verde sin pruebas | Mantén la verificación TRX y usa un proyecto de tests real |
| No existe un SDK del global.json | Alinea global.json con setup-dotnet |
| dotnet pack no funciona en VB clásico | Utiliza la ruta de empaquetado adecuada al formato del proyecto |
| Consumidor no puede restaurar | Configura fuente, credenciales, visibilidad y permisos de lectura |
| Publicación accidental desde un PR | Conserva la condición de evento/tag y no uses pull_request_target para ejecutar código no confiable con secretos |
| Falta una DLL/configuración | Inspecciona el contenido del nupkg y sus dependencias |

## 13. Próximo paso

Adapta las rutas, PackageId y el SDK; verifica que tu proyecto sea SDK-style y que tenga pruebas. Primero ejecuta CI sobre main y revisa el artifact. Después publica `v1.0.0` e instálalo desde otro proyecto.

**Validación de esta guía:** revisión de la estructura YAML/XML y de las dependencias entre jobs. No se ha ejecutado este workflow en tu repositorio: sus rutas, tests, políticas y permisos deben comprobarse con la primera ejecución de CI.
