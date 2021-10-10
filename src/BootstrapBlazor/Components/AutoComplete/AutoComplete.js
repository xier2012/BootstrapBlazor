(function ($) {
    $.extend({
        bb_autoScrollItem: function (el, index) {
            var $el = $(el);
            var $menu = $el.find('.dropdown-list');
            var maxHeight = parseInt($menu.css('max-height').replace('px', '')) / 2;
            var itemHeight = $menu.children('li:first').outerHeight();
            var height = itemHeight * index;
            var count = Math.floor(maxHeight / itemHeight);

            $menu.children().removeClass('active');
            $menu.children()[index].classList.add('active');

            if (height > maxHeight) {
                $menu.scrollTop(itemHeight * (index > count ? index - count : index));
            }
            else if (index <= count) {
                $menu.scrollTop(0);
            }
        },
        bb_debounce: function (ele, interval) {
            var handler = null;
            $(ele).on("input", function (event) {
                if (handler) {
                    window.clearTimeout(handler);
                    event.stopPropagation();
                    handler = window.setTimeout(function () {
                        window.clearTimeout(handler);
                        handler = null;
                        console.log('trigger', $(event.target).val());
                        event.target.dispatchEvent(event.originalEvent);
                    }, interval);
                } else {
                    console.log('stop', $(event.target).val());
                    handler = window.setTimeout(function () { }, interval);
                }
            });
        }
    });
})(jQuery);
