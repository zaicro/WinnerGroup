# NuGet público en Azure Artifacts: permisos, versiones y pipeline

Guía para partir de un usuario que puede iniciar sesión en Azure DevOps, pero cuyos permisos todavía no se han configurado.

**Resultado:** conservar en un feed privado los paquetes NuGet y las versiones que necesita tu aplicación, y restaurar esas mismas versiones en CI.

**Supuestos:** Azure DevOps Services (`dev.azure.com`), un proyecto privado y aplicaciones .NET que usan `PackageReference`. Si utilizas Azure DevOps Server, `packages.config` o proyectos antiguos que requieren MSBuild de Visual Studio, los comandos y los requisitos pueden cambiar. No se ha ejecutado esta configuración en tu organización.

## 1. Entender qué vas a configurar

| Elemento | Función |
|---|---|
| Azure Artifacts / feed privado | Guarda las versiones de paquetes que incorporas. |
| NuGet.org como upstream | Permite que el feed obtenga paquetes públicos y guarde una copia al restaurarlos con permisos adecuados. |
| `nuget.config` | Indica a la aplicación dónde buscar los paquetes. |
| `packages.lock.json` | Registra las versiones resueltas de dependencias directas y transitivas. |
| `dotnet restore --locked-mode` | Exige respetar el bloqueo durante CI. |

El feed no actualiza las referencias de tu aplicación. Tampoco evita por sí solo que alguien las modifique. El bloqueo se controla en el repositorio y en el pipeline. Un upstream habilitado permite incorporar nuevas versiones; no es una lista cerrada de paquetes aprobados. [1][2]

## 2. Primer paso: solicitar los permisos iniciales

**Poder entrar a Azure DevOps no significa poder administrar recursos. No puedes concederte permisos que todavía no tienes.** Un administrador de la organización y, según la tarea, un administrador del proyecto o propietario del feed debe hacer esta parte.

No necesitas ser administrador de la organización ni propietario de una suscripción de Azure para gestionar un feed concreto.

### 2.1. Acceso de tu usuario

Pide al administrador que revise:

1. **Organization settings → Users**: localizar tu cuenta y revisar su nivel de acceso.
2. Si también vas a modificar código en Azure Repos privados, asignar **Basic** o un nivel equivalente por suscripción. En Azure DevOps Services, Stakeholder permite acceder a Artifacts con los permisos correspondientes, pero no permite contribuir al código en Azure Repos privados. Basic no sustituye los permisos del feed. [3][4]
3. Agregarte al proyecto privado donde estará el feed. Si vas a modificar el repositorio, puede usarse el grupo **Contributors** del proyecto, respetando las restricciones del repositorio y las políticas de rama.
4. Si vas a crear o editar pipelines, habilitar los permisos correspondientes del proyecto/pipeline. Ser propietario de un feed no otorga automáticamente permisos de repositorio o de pipeline.

Si no aparece **Artifacts**, pide al administrador del proyecto que revise que el servicio esté habilitado en **Project settings → Overview → Services** y que tu cuenta tenga acceso al proyecto.

### 2.2. Quién crea el feed

Recomiendo esta ruta para tu situación:

1. El administrador crea un único feed privado, siguiendo el paso 3.
2. En **Artifacts → seleccionar feed → Feed settings → Permissions → Add users/groups**, agrega tu usuario.
3. Te asigna **Feed Owner** y guarda.

Así puedes configurar ese feed sin recibir administración de toda la organización.

Si necesitas crearlo tú, el administrador debe revisar **Azure Artifacts settings → Who can create feeds** y habilitarte según la política de la organización. Este ajuste pertenece a la configuración general de Artifacts; es distinto del engranaje de configuración de un feed. [5]

### 2.3. Elegir los roles del feed

| Identidad / necesidad | Rol del feed |
|---|---|
| Tu usuario, para configurar upstreams y permisos | **Feed Owner** |
| Usuario que incorpora paquetes nuevos desde NuGet.org | **Feed and Upstream Reader (Collaborator)** como mínimo |
| Usuario que sube manualmente archivos `.nupkg` | **Feed Publisher (Contributor)** como mínimo |
| Usuario que solo descarga paquetes ya guardados | **Feed Reader** |
| Pipeline que también podrá incorporar dependencias desde el upstream | **Feed and Upstream Reader (Collaborator)** |
| Pipeline limitado a paquetes que ya incorporó una persona autorizada | **Feed Reader** |

Estos son roles del feed. **Contributors del proyecto y Contributor del feed son permisos diferentes.** [5]

## 3. Crear el feed privado

Responsable: el administrador inicialmente, o tú si ya tienes autorización para crear feeds.

1. Entrar al proyecto privado.
2. Abrir **Artifacts → Create Feed**.
3. Nombre sugerido: `nuget-interno`.
4. Elegir visibilidad restringida a los usuarios/grupos necesarios.
5. Elegir **Scope: Project**, alojándolo en el mismo proyecto de la aplicación y del pipeline para simplificar permisos.
6. Activar **Upstream sources / Include packages from common public sources**, si aparece.
7. Crear el feed.
8. Revisar **Feed settings → Upstream sources** y confirmar que está **NuGet.org**.
9. Si falta: **Add upstream → Public source → NuGet.org → Add → Save**.

Tener paquetes de origen público no exige hacer público el feed. Se puede mantener privado. Si después varios proyectos deben compartirlo, evalúa un feed de organización y sus permisos antes de extender esta configuración. [1][6]

## 4. Dar acceso a la identidad del pipeline

**Tu sesión de usuario y el pipeline usan identidades distintas.** Que puedas descargar paquetes en tu equipo no prueba que el pipeline pueda hacerlo.

En **Artifacts → tu feed → Feed settings → Permissions**:

1. Seleccionar **Add users/groups**.
2. Buscar la identidad del proyecto:

   ```text
   NOMBRE_PROYECTO Build Service (NOMBRE_ORGANIZACION)
   ```

3. Asignar **Feed and Upstream Reader (Collaborator)** si el pipeline podrá guardar nuevas versiones desde el upstream.
4. Guardar.

Si el job utiliza identidad de colección en lugar de proyecto, revisar la identidad:

```text
Project Collection Build Service (NOMBRE_ORGANIZACION)
```

Concede el rol a la identidad que realmente ejecuta el job. Puedes consultar el alcance en **Project settings → Pipelines → Settings**, en la opción que limita la autorización del job al proyecto; puede existir una política a nivel de organización. No desactives esa limitación para solucionar un acceso al feed.

Si la identidad del proyecto no aparece, crea y ejecuta una primera vez un pipeline mínimo, por ejemplo con un paso `echo`, y vuelve a buscarla. Las identidades de proyecto pueden crearse con la primera ejecución.

Esta guía usa feed y pipeline en el mismo proyecto. Para un feed alojado en otro proyecto también hay que conceder acceso a ese proyecto; los permisos del feed por sí solos pueden no bastar. [5][7]

## 5. Obtener la dirección del feed

1. Abrir **Artifacts → nuget-interno → Connect to feed**.
2. Seleccionar **dotnet**.
3. Copiar la URL y seguir los requisitos de autenticación que muestre.

Un feed de proyecto tiene este formato:

```text
https://pkgs.dev.azure.com/ORGANIZACION/PROYECTO/_packaging/nuget-interno/nuget/v3/index.json
```

Usa la dirección exacta de **Connect to feed**. La URL de un feed de organización no lleva el segmento de proyecto. [7][8]

## 6. Configurar el repositorio

Los ejemplos usan `MiSolucion.sln`. Sustitúyelo por la ruta real de tu solución o `.csproj`. Ejecuta los comandos desde la raíz del repositorio.

### 6.1. Crear `nuget.config`

Guardar junto a la solución, reemplazando la URL:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="AzureArtifacts"
         value="https://pkgs.dev.azure.com/ORGANIZACION/PROYECTO/_packaging/nuget-interno/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

`<clear />` quita las fuentes heredadas de esa sección. Con esta configuración el proyecto solicita los paquetes al feed; el feed consulta NuGet.org mediante su upstream. No agregues NuGet.org como fuente directa de la aplicación si quieres que esos paquetes pasen por el feed. Revisa configuraciones adicionales dentro del repositorio y propiedades que puedan sobrescribir las fuentes.

No incluyas contraseñas ni tokens en este archivo. [8]

### 6.2. Declarar la versión que necesitas

Ejemplo ilustrativo en el `.csproj`; no es una recomendación de paquete ni de versión:

```xml
<ItemGroup>
  <PackageReference Include="Newtonsoft.Json" Version="[13.0.3]" />
</ItemGroup>
```

Los corchetes indican una versión exacta. `Version="13.0.3"` expresa un mínimo para la resolución de NuGet, aunque normalmente se restaure esa versión cuando está disponible. Evita comodines como `13.*` si buscas fijar la versión. [9]

Si ya utilizas administración central de paquetes, define la versión en `Directory.Packages.props`, en el `PackageVersion` correspondiente, y conserva el `PackageReference` sin versión. No mezcles ambos mecanismos sin revisar la configuración existente.

### 6.3. Habilitar el archivo de bloqueo

En el `.csproj` de la aplicación, agregar dentro de `<Project>`:

```xml
<PropertyGroup>
  <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
</PropertyGroup>
```

Para habilitarlo en todos los proyectos del repositorio puedes usar un `Directory.Build.props` en la raíz con esa propiedad. Si el archivo ya existe, integra la propiedad; no lo reemplaces.

El bloqueo de una biblioteca no impone sus versiones a una aplicación que la consume: la aplicación debe bloquear su propia resolución. [2]

## 7. Autenticar tu equipo

Necesitas el SDK de .NET compatible con tu proyecto y el proveedor de credenciales de Azure Artifacts.

Para el instalador actual como herramienta de dotnet, la documentación exige SDK **9.0.200 o posterior**. Puedes consultar tu instalación con `dotnet --info`. Si el proyecto usa un SDK anterior, sigue las instrucciones compatibles de **Connect to feed** o del proveedor de credenciales; no cambies el framework de la aplicación solo para instalar el proveedor. [8]

En un equipo compatible, instala el proveedor:

```bash
dotnet tool install --global Microsoft.Artifacts.CredentialProvider.NuGet.Tool --source https://api.nuget.org/v3/index.json
```

Esta descarga directa de NuGet.org instala la herramienta de autenticación. Las dependencias de la aplicación seguirán usando el feed configurado.

En la primera restauración usa `--interactive` e inicia sesión con tu usuario autorizado. La instalación del proveedor no concede permisos: tu cuenta todavía debe tener los roles descritos antes. En CI se usará `NuGetAuthenticate@1`, sin inicio de sesión interactivo. [8][10]

## 8. Incorporar los paquetes y sus versiones al feed

Una restauración puede resolver todo desde la caché local y no contactar al feed. Para la carga inicial, utiliza una carpeta de paquetes nueva y desactiva la caché HTTP. [11]

1. Añade a `.gitignore` esta carpeta temporal:

   ```gitignore
   .nuget-feed-bootstrap/
   ```

2. Verifica que esa carpeta todavía no exista o esté vacía.
3. Ejecuta con tu usuario Owner o Collaborator:

   ```bash
   dotnet restore MiSolucion.sln --configfile nuget.config --interactive --use-lock-file --force --no-http-cache --packages ./.nuget-feed-bootstrap
   ```

4. Completa el inicio de sesión cuando se solicite.
5. Abre **Artifacts → nuget-interno** y comprueba que aparecen los paquetes y las versiones resueltas, incluidas sus dependencias transitivas.
6. Revisa los `packages.lock.json` generados junto a los proyectos. Confirma que registran las versiones esperadas.

**No necesitas descargar y subir cada `.nupkg` manualmente.** El feed guarda los paquetes que se descargan mediante el upstream con una identidad Collaborator o superior. Un Reader solamente puede descargar los que ya están guardados. [1]

Si estás conservando una aplicación existente, compara también con su resolución anterior; no asumas que una referencia de versión mínima equivale a lo que estaba desplegado.

## 9. Guardar la configuración en Git

Revisa e incorpora mediante el flujo de ramas/PR del equipo:

- `nuget.config`.
- Cambios en `.csproj` o `Directory.Packages.props`.
- `Directory.Build.props`, si lo usaste.
- `packages.lock.json` de los proyectos cuyo bloqueo aplica en CI, especialmente la aplicación final.
- Ajuste de `.gitignore`.
- YAML del pipeline.

No incorpores la carpeta de paquetes, `bin`, `obj`, tokens ni credenciales. Mantén el SDK seleccionado mediante `global.json` si también quieres controlar cambios de herramienta; el lock de NuGet no fija la versión del SDK.

## 10. Configurar Azure Pipelines

En tu pipeline existente, agrega la autenticación antes de restaurar. El agente debe tener instalado el SDK requerido por la aplicación; usa `UseDotNet@2` si necesitas instalarlo explícitamente. `NuGetAuthenticate@1` requiere un runtime .NET compatible con la tarea. [10]

Ejemplo de pasos para una solución compatible con `dotnet`:

```yaml
steps:
- checkout: self

- task: NuGetAuthenticate@1
  displayName: Autenticar Azure Artifacts

- script: dotnet restore MiSolucion.sln --configfile nuget.config --locked-mode
  displayName: Restaurar versiones bloqueadas

- script: dotnet build MiSolucion.sln --configuration Release --no-restore
  displayName: Compilar

- script: dotnet test MiSolucion.sln --configuration Release --no-build --no-restore
  displayName: Ejecutar pruebas
```

Adapta la ruta y el paso de pruebas a tu solución. En feeds de la misma organización no necesitas agregar un PAT personal a este YAML: la tarea utiliza las credenciales del job y la identidad del pipeline debe tener acceso al feed. [10]

`--locked-mode` hace fallar la restauración si los cambios de dependencias requieren modificar el bloqueo. `--no-restore` en los pasos posteriores evita otra restauración implícita fuera de ese paso. El pipeline debe fallar ante un error de restore; no configures `continueOnError` para esa tarea. [2]

## 11. Validar antes de darlo por terminado

- [ ] Tu usuario puede entrar al proyecto y al feed.
- [ ] Tu usuario es Owner del feed si administrará upstreams y permisos.
- [ ] NuGet.org aparece como upstream.
- [ ] La aplicación tiene únicamente el feed interno como fuente efectiva para estos paquetes.
- [ ] Los paquetes y las versiones esperadas aparecen en Artifacts.
- [ ] Las dependencias transitivas también se incorporaron.
- [ ] Los archivos de bloqueo están revisados y guardados en Git.
- [ ] La identidad del pipeline tiene el rol adecuado.
- [ ] Una ejecución del pipeline restaura, compila y ejecuta las pruebas.

Para comprobar la disponibilidad sin depender de la caché habitual, repite la restauración con `--locked-mode --no-http-cache --force --packages` y otra carpeta nueva. Si quieres comprobar que todo ya está guardado, haz esa prueba con una identidad **Feed Reader**: no podrá incorporar paquetes faltantes desde el upstream. [1][11]

## 12. Si necesitas admitir solo paquetes previamente aprobados

La configuración anterior permite conservar y bloquear dependencias. Para impedir que los pipelines incorporen versiones nuevas:

1. Una persona autorizada incorpora y valida las versiones usando Owner o Collaborator.
2. Los consumidores ordinarios y la identidad del pipeline reciben **Feed Reader**.
3. Revisa también los roles heredados de grupos: agregar Reader no elimina permisos superiores que la identidad ya tenga.
4. Mantén el archivo de bloqueo y el restore en modo bloqueado.

Esto hace que una dependencia todavía no incorporada falle en CI. Si necesitas un catálogo formal de aprobación separado de la incorporación inicial, diseña un flujo de promoción mediante vistas o un feed separado. No asumas que todos los paquetes presentes en un feed con upstream ya fueron revisados. [1][5]

## 13. Cómo actualizar una dependencia de forma controlada

1. Crear una rama y cambiar deliberadamente la versión requerida.
2. Restaurar con una identidad autorizada, sin `--locked-mode`, para actualizar el bloqueo e incorporar las nuevas dependencias al feed.
3. Revisar el cambio de versión y el diff de los archivos de bloqueo.
4. Compilar y ejecutar las pruebas relevantes.
5. Abrir el PR con el cambio de referencia y de bloqueo.
6. Ejecutar CI en modo bloqueado y aprobar por el procedimiento habitual.

Conservar versiones conocidas ayuda a la estabilidad. No significa congelarlas indefinidamente: mantén una revisión periódica de vulnerabilidades y compatibilidad.

## 14. Errores habituales

| Síntoma | Qué revisar |
|---|---|
| Puedo entrar a DevOps pero no al proyecto | Membresía y permisos del proyecto privado. |
| No puedo crear el feed | Política **Who can create feeds**; solicitar creación al administrador. |
| No puedo editar upstreams o permisos | Rol **Feed Owner** en ese feed. |
| `401 Unauthorized` | Proveedor de credenciales, cuenta utilizada y autenticación interactiva; en CI, tarea de autenticación. |
| `403 Forbidden` | Roles de tu usuario o de la identidad real del job. |
| Paquete no encontrado | URL y alcance del feed, versión exacta existente, upstream habilitado y permiso para la primera incorporación. |
| Restore funciona pero el paquete no aparece en Artifacts | Caché local, otra fuente efectiva o restore que no necesitó descargar; repetir el paso 8 con carpeta nueva. |
| Funciona en mi equipo pero falla en CI | Identidad Build Service, autenticación y SDK del agente. |
| Restore bloqueado falla tras editar dependencias | Actualizar y revisar el lock localmente, incorporarlo al PR; no quitar el bloqueo de CI como solución. |

Estas comprobaciones orientan el diagnóstico; el código HTTP por sí solo no identifica siempre la causa. [12]

Revisa también las políticas de retención del feed: si necesitas conservar versiones usadas por aplicaciones antiguas, evita que una limpieza las elimine. Consulta el almacenamiento disponible antes de incorporar grandes conjuntos de paquetes.

## 15. Orden recomendado para tu caso

1. **Ahora:** conseguir acceso al proyecto y pedir que creen el feed privado con tu usuario como Owner.
2. Confirmar upstream y permisos del Build Service.
3. Probar con una aplicación pequeña y una versión conocida, siguiendo los pasos 5 a 8.
4. Incorporar el bloqueo y activar CI.
5. Extenderlo al resto de aplicaciones después de verificar el primer caso.

Deja de considerar la copia del `.nupkg` como el mecanismo que impide actualizaciones: la copia conserva disponibilidad; la referencia y el bloqueo controlan las versiones.

## Referencias oficiales

Documentación consultada el 27 de octubre de 2026. Las etiquetas de la interfaz pueden variar según idioma y edición.

1. [Upstream sources y guardado de paquetes](https://learn.microsoft.com/en-us/azure/devops/artifacts/how-to/set-up-upstream-sources?view=azure-devops)
2. [PackageReference: archivos de bloqueo y locked mode](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
3. [Administrar usuarios y niveles de acceso](https://learn.microsoft.com/en-us/azure/devops/organizations/accounts/add-organization-users?view=azure-devops)
4. [Alcance de Stakeholder](https://learn.microsoft.com/en-us/azure/devops/organizations/security/stakeholder-access?view=azure-devops)
5. [Permisos de Azure Artifacts y roles de feed](https://learn.microsoft.com/en-us/azure/devops/artifacts/feeds/feed-permissions?view=azure-devops)
6. [Crear feeds y elegir alcance](https://learn.microsoft.com/en-us/azure/devops/artifacts/concepts/feeds?view=azure-devops)
7. [Feeds de proyecto y de organización](https://learn.microsoft.com/en-us/azure/devops/artifacts/feeds/project-scoped-feeds?view=azure-devops)
8. [Conectar dotnet a Azure Artifacts](https://learn.microsoft.com/en-us/azure/devops/artifacts/nuget/dotnet-setup?view=azure-devops)
9. [Versiones y rangos de NuGet](https://learn.microsoft.com/en-us/nuget/concepts/package-versioning)
10. [Tarea NuGetAuthenticate@1](https://learn.microsoft.com/en-us/azure/devops/pipelines/tasks/reference/nuget-authenticate-v1?view=azure-pipelines)
11. [Opciones de dotnet restore](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore) y [cachés de NuGet](https://learn.microsoft.com/en-us/nuget/consume-packages/managing-the-global-packages-and-cache-folders)
12. [Resolver problemas de Azure Artifacts](https://learn.microsoft.com/en-us/azure/devops/artifacts/troubleshooting)
