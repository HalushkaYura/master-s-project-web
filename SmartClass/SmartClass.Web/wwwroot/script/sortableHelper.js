window.initializeSortable = (dotNetObjectReference) => {
    var columns = document.querySelectorAll('.task-list');

    columns.forEach(column => {
        new Sortable(column, {
            group: 'shared',
            animation: 150,
            onEnd: async (evt) => {
                const item = evt.item;
                const fromStatus = evt.from.getAttribute('data-status');
                const toStatus = evt.to.getAttribute('data-status');
                const taskId = item.getAttribute('data-task-id');
                await dotNetObjectReference.invokeMethodAsync('OnTaskDrop', taskId, fromStatus, toStatus);
            }
        });
    });
};