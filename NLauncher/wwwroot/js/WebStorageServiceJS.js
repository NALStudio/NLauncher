/**
 * @param {string} value
 */
export function setLocalStorageValue(value) {
    localStorage.setItem("NLauncher.WebStorageService", value);
}

export function getLocalStorageValue() {
    return localStorage.getItem("NLauncher.WebStorageService");
}