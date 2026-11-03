(function () {
    'use strict';
    // Swagger UI 2 integration. Secrets remain in its in-memory authorization registry.
    function install() {
        if (document.getElementById('cirsa-authorize')) { return; }
        var previousFocus;
        var style = document.createElement('style');
        style.textContent = '.cirsa-auth-bar{padding:12px 20px;background:#eef2f6;display:flex;gap:14px;align-items:center;font:14px Arial,sans-serif}.cirsa-auth-button{padding:8px 16px;border:1px solid #496a89;border-radius:4px;background:#fff;color:#17334d;cursor:pointer}.cirsa-auth-overlay{position:fixed;inset:0;z-index:10000;background:rgba(0,0,0,.45);display:flex;align-items:center;justify-content:center}.cirsa-auth-overlay[hidden]{display:none}.cirsa-auth-dialog{box-sizing:border-box;background:white;color:#222;border-radius:6px;padding:24px;width:min(520px,94vw);box-shadow:0 12px 40px #333;font:14px Arial,sans-serif}.cirsa-auth-dialog h2{font-size:22px;margin:0 0 20px}.cirsa-auth-dialog label{display:block;margin-top:16px}.cirsa-auth-dialog input{box-sizing:border-box;width:100%;padding:10px;margin:6px 0;border:1px solid #aaa}.cirsa-auth-actions{display:flex;gap:8px;flex-wrap:wrap;margin-top:20px}.cirsa-auth-dialog p{line-height:1.5;margin:8px 0}';
        document.head.appendChild(style);
        function element(tag, id, text) {
            var node = document.createElement(tag);
            if (id) { node.id = id; }
            if (text) { node.textContent = text; }
            return node;
        }
        var bar = element('div'); bar.className = 'cirsa-auth-bar';
        var open = element('button', 'cirsa-authorize', 'Authorize');
        open.type = 'button'; open.className = 'cirsa-auth-button';
        var state = element('span', 'cirsa-auth-state', 'No credentials configured.');
        state.setAttribute('aria-live', 'polite');
        bar.appendChild(open); bar.appendChild(state);
        document.body.insertBefore(bar, document.body.firstChild);

        var overlay = element('div', 'cirsa-auth-overlay');
        overlay.className = 'cirsa-auth-overlay'; overlay.hidden = true;
        var dialog = element('div'); dialog.className = 'cirsa-auth-dialog';
        dialog.setAttribute('role', 'dialog'); dialog.setAttribute('aria-modal', 'true');
        dialog.setAttribute('aria-labelledby', 'cirsa-auth-title');
        dialog.appendChild(element('h2', 'cirsa-auth-title', 'Authorize requests'));
        dialog.appendChild(element('p', null, 'Credentials are kept only in this page memory. Leave a field empty to remove that credential.'));
        function credentialInput(id, labelText) {
            var label = element('label', null, labelText); label.setAttribute('for', id);
            var input = element('input', id); input.type = 'password'; input.autocomplete = 'off';
            input.setAttribute('spellcheck', 'false');
            dialog.appendChild(label); dialog.appendChild(input); return input;
        }
        var key = credentialInput('cirsa-api-key', 'API key (X-Api-Key)');
        var jwt = credentialInput('cirsa-jwt', 'JWT (with or without the Bearer prefix)');
        var message = element('p', 'cirsa-auth-message'); message.setAttribute('aria-live', 'polite'); dialog.appendChild(message);
        var actions = element('div'); actions.className = 'cirsa-auth-actions';
        function button(id, text) { var b = element('button', id, text); b.type = 'button'; b.className = 'cirsa-auth-button'; actions.appendChild(b); return b; }
        var apply = button('cirsa-auth-apply', 'Apply');
        var clear = button('cirsa-auth-clear', 'Clear credentials');
        var close = button('cirsa-auth-close', 'Close');
        dialog.appendChild(actions); overlay.appendChild(dialog); document.body.appendChild(overlay);
        function resetInputs() { key.value = ''; jwt.value = ''; }
        function hide() { resetInputs(); message.textContent = ''; overlay.hidden = true; if (previousFocus) { previousFocus.focus(); } }
        function registry() {
            var ui = window.swaggerUi;
            if (!ui || !ui.api || !ui.api.clientAuthorizations) {
                throw new Error('Swagger UI is not ready. Wait for the document to load and retry.');
            }
            return ui.api.clientAuthorizations;
        }
        // Swagger UI 2 accepts authorization objects exposing apply(request).
        // Avoid relying on SwaggerClient constructors that are not browser globals.
        function headerAuthorization(name, value) {
            return { apply: function (request) {
                request.headers = request.headers || {};
                if (typeof request.headers[name] === 'undefined') { request.headers[name] = value; }
                return true;
            } };
        }
        open.onclick = function () { previousFocus = document.activeElement; resetInputs(); message.textContent = ''; overlay.hidden = false; key.focus(); };
        close.onclick = hide;
        overlay.onclick = function (event) { if (event.target === overlay) { hide(); } };
        dialog.onkeydown = function (event) {
            if (event.key === 'Escape' || event.keyCode === 27) { event.preventDefault(); hide(); }
            if (event.key === 'Tab' || event.keyCode === 9) {
                var first = key; var last = close;
                if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
                else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
            }
        };
        apply.onclick = function () {
            try {
                var client = registry();
                var keyValue = key.value.trim();
                var tokenValue = jwt.value.trim().replace(/^Bearer\s+/i, '');
                client.remove('ApiKey'); client.remove('Bearer');
                if (keyValue) { client.add('ApiKey', headerAuthorization('X-Api-Key', keyValue)); }
                if (tokenValue) { client.add('Bearer', headerAuthorization('Authorization', 'Bearer ' + tokenValue)); }
                state.textContent = keyValue && tokenValue ? 'API key and JWT configured.' : keyValue ? 'API key configured.' : tokenValue ? 'JWT configured.' : 'No credentials configured.';
                hide();
            } catch (error) { message.textContent = error.message; }
        };
        clear.onclick = function () {
            try { var client = registry(); client.remove('ApiKey'); client.remove('Bearer'); state.textContent = 'No credentials configured.'; hide(); }
            catch (error) { message.textContent = error.message; }
        };
    }
    if (document.readyState === 'loading') { document.addEventListener('DOMContentLoaded', install); }
    else { install(); }
}());
