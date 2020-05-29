/*$(function () {
    var fixblock_height = $('#buttons').height(); // ���������� ������ �������������� �����
    var fixblock_pos = $('#buttons').position().top; ; // ���������� ��� �������������� ���������
    var fixblock_width = $('#buttons').width(); // ���������� ������ �������������� �����
    var fixblock_wpos = $('#buttons').position().left; ; // ���������� ��� �������������� ���������
    $(window).scroll(function () {
        if ($(window).scrollTop() > fixblock_pos) { // ���� �������� ���������� ������, ��� ��������� ��� ����
            $('#buttons').css({ 'position': 'fixed', 'top': '0px', 'z-index': '10' }); // �� �� ���� ���� ��������� � ���������� ������.
            $('#header').css('padding-bottom', fixblock_height + 'px'); // � ����� ��� ���� ������, ��������� ����� ����� ��� �������� ����� (��� ����� ���� ���� ��� ��� ��� ����)
        } 
        if ($(window).scrollLeft() > fixblock_wpos) { // ���� �������� ���������� ������, ��� ��������� ��� ����
            $('#buttons').css({ 'position': 'fixed', 'left': '0px', 'z-index': '10', 'top' : fixblock_pos + 'px' }); // �� �� ���� ���� ��������� � ���������� ������.
            $('#header').css({ 'position': 'fixed', 'left': '0px', 'z-index': '10', 'top': fixblock_pos - 70 + 'px',
                'width': '100%' }); // �� �� ���� ���� ��������� � ���������� ������.
        }
        if ($(window).scrollTop() = fixblock_pos && $(window).scrollLeft() = fixblock_wpos) {  // ���� �� ������� ������ ������ (����), ��� ��� ����
            $('#buttons').css({ 'position': 'static' }); // �� ���������� ��� �����
            $('#header').css({ 'position': 'static' }); // �� ���������� ��� �����
        }
    })
});	
*/
$(function () {
    var bar_height = $('#buttons').height(); // ���������� ������ �������������� �����
    //var menu_top = $('#sideLeft').position().top(); // ���������� ������ �������������� �����
    var header_height = $('#header').height(); // ���������� ������ �������������� �����
    var header_bottom = $('#header').position().bottom; // ���������� ������ �������������� �����
    var window_height = $(window).height();   // returns height of browser viewport

    if (header_bottom > bar_height) {
        $('#sideLeft').css({ 'height': window_height - header_bottom + 'px' });
    } else {
        $('#sideLeft').css({ 'height': window_height - bar_height + 'px' });
    }
});	