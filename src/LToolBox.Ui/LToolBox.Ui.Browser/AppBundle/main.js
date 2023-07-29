// StateManager
/**
 * Save the app state
 * @param {string} state The app state as text
 */
function save(state) {
    let storage = window.localStorage;
    storage.setItem("config", state);
}

/**
 * Load the app state as text
 * @return {string} The app stored state as text
 */
function load() {
    let storage = window.localStorage;
    return storage.getItem("config");
}

/**
 * Invalidate the app state
 */
function invalidate() {
    let storage = window.localStorage;
    storage.removeItem("config");
}

import {dotnet} from './dotnet.js'

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);


const {setModuleImports, getAssemblyExports, getConfig} = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

setModuleImports("stateManager", {
    save: save,
    load: load,
    invalidate: invalidate
});

const exports = await getAssemblyExports(config.mainAssemblyName);
const onBeforeUnloadCallback = exports.JavascriptStateManager.OnBeforeUnload
window.onbeforeunload = onBeforeUnloadCallback;

const config = getConfig();

await dotnetRuntime.runMainAndExit(config.mainAssemblyName, [window.location.search]);