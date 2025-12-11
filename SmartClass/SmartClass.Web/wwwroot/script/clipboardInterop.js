window.copyTextToClipboard = function (text) {
    navigator.clipboard.writeText(text);
};

        $(function () {
            $('[data-toggle="popover"]').popover({
                trigger: 'hover',
                placement: 'auto',
                html: true
            });
        });


//window.addEventListener('DOMContentLoaded', () => {
//    initDragAndDrop();
//});

//function initDragAndDrop() {
//    const items = document.querySelectorAll('[draggable=true]');
//    items.forEach(item => {
//        item.addEventListener('dragstart', e => {
//            e.dataTransfer.setData('text/plain', ''); // потрібно для перетягування в Firefox
//        });
//    });
//}