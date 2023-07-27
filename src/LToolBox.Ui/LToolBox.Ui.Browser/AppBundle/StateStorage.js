/**
 * Save the app state
 * @param {string} state The app state as text
 */
function save(state){
    let storage = window.localStorage;
    storage.setItem("config", state);
}

/**
 * Load the app state as text
 * @return {string} The app stored state as text
 */
function load(){
    let storage = window.localStorage;
    return storage.getItem("config");
}

/**
 * Invalidate the app state
 */
function invalidate(){
    let storage = window.localStorage;
    storage.removeItem("config");
}