/*$(function () {
    var fixblock_height = $('#buttons').height(); // определяем высоты фиксированного блока
    var fixblock_pos = $('#buttons').position().top; ; // определяем его первоначальное положение
    var fixblock_width = $('#buttons').width(); // определяем ширину фиксированного блока
    var fixblock_wpos = $('#buttons').position().left; ; // определяем его первоначальное положение
    $(window).scroll(function () {
        if ($(window).scrollTop() > fixblock_pos) { // Если страницу прокрутили дальше, чем находится наш блок
            $('#buttons').css({ 'position': 'fixed', 'top': '0px', 'z-index': '10' }); // То мы этот блок фиксируем и отображаем сверху.
            $('#header').css('padding-bottom', fixblock_height + 'px'); // А чтобы это было плавно, добавляем отсуп снизу для верхнего блока (как будто этот блок там все еще есть)
        } 
        if ($(window).scrollLeft() > fixblock_wpos) { // Если страницу прокрутили дальше, чем находится наш блок
            $('#buttons').css({ 'position': 'fixed', 'left': '0px', 'z-index': '10', 'top' : fixblock_pos + 'px' }); // То мы этот блок фиксируем и отображаем сверху.
            $('#header').css({ 'position': 'fixed', 'left': '0px', 'z-index': '10', 'top': fixblock_pos - 70 + 'px',
                'width': '100%' }); // То мы этот блок фиксируем и отображаем сверху.
        }
        if ($(window).scrollTop() = fixblock_pos && $(window).scrollLeft() = fixblock_wpos) {  // Если же позиция скрола меньше (выше), чем наш блок
            $('#buttons').css({ 'position': 'static' }); // то возвращаем все назад
            $('#header').css({ 'position': 'static' }); // то возвращаем все назад
        }
    })
});	
*/
$(function () {
    var bar_height = $('#buttons').height(); // определяем высоты фиксированного блока
    //var menu_top = $('#sideLeft').position().top(); // определяем высоты фиксированного блока
    var header_height = $('#header').height(); // определяем ширину фиксированного блока
    var header_bottom = $('#header').position().bottom; // определяем ширину фиксированного блока
    var window_height = $(window).height();   // returns height of browser viewport

    if (header_bottom > bar_height) {
        $('#sideLeft').css({ 'height': window_height - header_bottom + 'px' });
    } else {
        $('#sideLeft').css({ 'height': window_height - bar_height + 'px' });
    }
});	