// Keyboard event listener for modals
window.addKeyboardListener = (dotNetRef) => {
    const handler = (e) => {
        if (e.key === 'Escape') {
            dotNetRef.invokeMethodAsync('HandleKeyPress', 'Escape');
        }
    };
    
    document.addEventListener('keydown', handler);
    
    // Store the handler so we can remove it later
    window.currentKeyboardHandler = handler;
};

window.removeKeyboardListener = () => {
    if (window.currentKeyboardHandler) {
        document.removeEventListener('keydown', window.currentKeyboardHandler);
        window.currentKeyboardHandler = null;
    }
};

// Focus element by ID
window.focusElement = (elementId) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
};