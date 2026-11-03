'use strict';
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const { JSDOM } = require('jsdom');
const source = fs.readFileSync(path.join(__dirname, '../../Swagger/Assets/authorization.js'), 'utf8');
const dom = new JSDOM('<!doctype html><html><head></head><body></body></html>', { runScripts: 'outside-only', url: 'https://localhost/swagger/ui/index' });
const { window } = dom;
const document = window.document;
const entries = {};
// This is the add/remove/apply contract of Swagger UI 2's clientAuthorizations.
window.swaggerUi = { api: { clientAuthorizations: {
  add: (name, value) => { entries[name] = value; },
  remove: name => delete entries[name]
} } };
Object.defineProperty(window, 'localStorage', { get: () => { throw new Error('Persistent storage is forbidden'); } });
Object.defineProperty(window, 'sessionStorage', { get: () => { throw new Error('Persistent storage is forbidden'); } });
window.eval(source);
document.dispatchEvent(new window.Event('DOMContentLoaded'));
const el = id => document.getElementById(id);
el('cirsa-authorize').click();
assert.equal(el('cirsa-auth-overlay').hidden, false);
assert.equal(document.activeElement.id, 'cirsa-api-key');
el('cirsa-api-key').value = 'test-key';
el('cirsa-jwt').value = 'Bearer test.jwt.value';
el('cirsa-auth-apply').click();
assert.equal(el('cirsa-auth-overlay').hidden, true);
assert.equal(el('cirsa-api-key').value, '');
assert.equal(el('cirsa-jwt').value, '');
const request = { headers: {}, url: '/test' };
entries.ApiKey.apply(request);
entries.Bearer.apply(request);
assert.deepEqual(request.headers, { 'X-Api-Key': 'test-key', Authorization: 'Bearer test.jwt.value' });
assert.equal(window.location.href, 'https://localhost/swagger/ui/index');
assert.equal(document.body.textContent.includes('test.jwt.value'), false);
el('cirsa-authorize').click();
el('cirsa-jwt').dispatchEvent(new window.KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
assert.equal(el('cirsa-auth-overlay').hidden, true);
assert.equal(Object.keys(entries).length, 2); // Closing is not revocation.
el('cirsa-authorize').click();
el('cirsa-auth-clear').click();
assert.equal(Object.keys(entries).length, 0);
el('cirsa-authorize').click();
el('cirsa-jwt').value = 'jwt.only';
el('cirsa-auth-apply').click();
assert.deepEqual(Object.keys(entries), ['Bearer']);
const jwtOnly = { headers: {} }; entries.Bearer.apply(jwtOnly);
assert.equal(jwtOnly.headers.Authorization, 'Bearer jwt.only');
window.eval(source); document.dispatchEvent(new window.Event('DOMContentLoaded'));
assert.equal(document.querySelectorAll('#cirsa-authorize').length, 1);
// A missing/unready Swagger client reports an error and keeps the dialog open.
window.swaggerUi = null;
el('cirsa-authorize').click(); el('cirsa-auth-apply').click();
assert.equal(el('cirsa-auth-overlay').hidden, false);
assert.match(el('cirsa-auth-message').textContent, /not ready/);
console.log('PASS authorization dialog: apply, clear, JWT prefix, focus, escape, no storage, duplicate install, unready client.');
dom.window.close();
