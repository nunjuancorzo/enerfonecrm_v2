export function desplazar(id) {
    const conversation = document.getElementById(id);
    if (conversation) conversation.scrollTop = conversation.scrollHeight;
}