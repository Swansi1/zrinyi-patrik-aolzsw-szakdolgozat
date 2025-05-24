let pager = (function pager() {
    let currentPage = 1;
    const limit = 1;
    let totalCount = 0;
    let activeElement = [];
    let callBackFunction = null;

    function init(callBack) {
        callBackFunction = callBack;

        _loadSkeleton();
        renderPager();

        $(document).on('click', '.pagination .page-link', function (e) {
            e.preventDefault();
            const page = +$(this).data('page');
            if (!isNaN(page) && page >= 1 && page <= Math.ceil(totalCount / limit)) {
                currentPage = page;
                callBackFunction(page);
            }
        });
    }

    function renderPager() {
        const pageCount = Math.ceil(totalCount / limit);
        const $ul = $('.pagination').empty();

        if (pageCount <= 1) return;

        // helper to render a page item
        const pageItem = (label, page, isActive, isDisabled) => `
        <li class="page-item
                   ${isActive ? 'active' : ''}
                   ${isDisabled ? 'disabled' : ''}">
          <a class="page-link" href="#"
             data-page="${page}"
             tabindex="${isDisabled ? '-1' : null}">
            ${label}
          </a>
        </li>`;

        // Previous button
        $ul.append(pageItem('«', currentPage - 1, false, currentPage === 1));

        // Always show 1
        $ul.append(pageItem('1', 1, currentPage === 1, false));

        // Left ellipsis
        if (currentPage - 3 > 1) {
            $ul.append('<li class="page-item disabled"><span class="page-link">…</span></li>');
        }

        // Pages around current: from (current−2) to (current+2)
        const start = Math.max(2, currentPage - 1);
        const end = Math.min(pageCount - 1, currentPage + 1);
        for (let p = start; p <= end; p++) {
            $ul.append(pageItem(p, p, p === currentPage, false));
        }

        // Right ellipsis
        if (currentPage + 3 < pageCount) {
            $ul.append('<li class="page-item disabled"><span class="page-link">…</span></li>');
        }

        // Always show last page
        if (pageCount > 1) {
            $ul.append(pageItem(pageCount, pageCount, currentPage === pageCount, false));
        }

        // Next button
        $ul.append(pageItem('»', currentPage + 1, false, currentPage === pageCount));
    }

    function _loadSkeleton() {
        $(".pager").html(`
            <div class="d-flex justify-content-center mt-3">
                <nav>
                    <ul class="pagination"></ul>
                </nav>
            </div>`);
    }
    
    function setTotalCount(count) {
        totalCount = count;
    }
    
    return {
        init: init,
        renderPager: renderPager,
        setTotalCount: setTotalCount,
        currentPage: currentPage,
        limit: limit,
    };
})();
