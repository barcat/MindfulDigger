export function setupInfiniteScroll(context) {
    const options = {
        root: null,
        rootMargin: '0px',
        threshold: 0.1
    };
    context.observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting && !context.isLoading && context.hasMorePages) {
                context.loadMoreNotes();
            }
        });
    }, options);
    const trigger = document.getElementById('scroll-trigger');
    if (trigger) {
        context.observer.observe(trigger);
    }
}
