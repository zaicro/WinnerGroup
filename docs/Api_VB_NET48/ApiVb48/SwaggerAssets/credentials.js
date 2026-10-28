//(function () {
//    'use strict';
//    // Swagger UI 2: agrega ambas cabeceras en memoria, sin localStorage.
//    var attempts = 0;
//    function install() {
//        if (!window.swaggerUi || !window.swaggerUi.api) {
//            if (++attempts < 120) { window.setTimeout(install, 250); }
//            return;
//        }
//        var panel = document.createElement('div');
//        panel.style.cssText = 'padding:15px;margin:10px auto;max-width:960px;background:#eef2f6';
//        panel.innerHTML = '<label>API key <input id="app-api-key" type="password" autocomplete="off"></label> ' +
//            '<label>JWT (sin Bearer) <input id="app-jwt" type="password" autocomplete="off"></label> ' +
//            '<button id="app-apply" type="button">Aplicar credenciales</button> ' +
//            '<button id="app-clear" type="button">Limpiar</button> <span id="app-state"></span>';
//        document.body.insertBefore(panel, document.body.firstChild);
//        document.getElementById('app-apply').onclick = function () {
//            var auth = window.swaggerUi.api.clientAuthorizations;
//            var key = document.getElementById('app-api-key').value.trim();
//            var token = document.getElementById('app-jwt').value.trim();
//            auth.remove('ApiKey'); auth.remove('Bearer');
//            if (key) { auth.add('ApiKey', new SwaggerClient.ApiKeyAuthorization('X-Api-Key', key, 'header')); }
//            if (token) { auth.add('Bearer', new SwaggerClient.ApiKeyAuthorization('Authorization', 'Bearer ' + token, 'header')); }
//            document.getElementById('app-state').textContent = 'Credenciales aplicadas en memoria.';
//        };
//        document.getElementById('app-clear').onclick = function () {
//            document.getElementById('app-api-key').value = '';
//            document.getElementById('app-jwt').value = '';
//            document.getElementById('app-apply').click();
//        };
//    }
//    install();
//}());
