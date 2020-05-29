var clear = "img/clear.gif"; //path to clear.gif
document.write(
	'<script type="text/javascript" id="ct" defer="defer" src="javascript:void(0)"><\/script>'
	);
var ct = document.getElementById("ct");
ct.onreadystatechange = function () { pngfix() };
pngfix = function () {
    var els = document.getElementsByTagName('*'), ip = /\.png/i, al = "progid:DXImageTransform.Microsoft.AlphaImageLoader(src='", i = els.length, uels = new Array(), c = 0; while (i-- > 0) { if (els[i].className.match(/unitPng/)) { uels[c] = els[i]; c++; } }
    if (uels.length == 0) pfx(els); else pfx(uels);
    function pfx(els) {
        i = els.length;
        while (i-- > 0) {
            var el = els[i], es = el.style, elc = el.currentStyle, elb = elc.backgroundImage;
            if (el.src && el.src.match(ip) && !es.filter) {
                es.height = el.height; es.width = el.width; es.filter = al + el.src + "',sizingMethod='crop')";
                el.src = clear;
            } else {
                if (elb.match(ip)) {
                    var path = elb.split('"'), rep = (elc.backgroundRepeat == 'no-repeat') ? 'crop' : 'scale', elkids = el.getElementsByTagName('*'), j = elkids.length; es.filter = al + path[1] + "',sizingMethod='" + rep + "')";
                    es.height = el.clientHeight + 'px';
                    es.backgroundImage = 'none';
                    if (j != 0) {
                        if (elc.position != "absolute") es.position = 'static';
                        while (j-- > 0) if (!elkids[j].style.position) elkids[j].style.position = "relative";
                    } 
                } 
            } 
        } 
    };
};

var array = new Array();
var speed = 10;
var timer = 10;

// Loop through all the divs in the slider parent div //
// Calculate seach content divs height and set it to a variable //
function slider(target, showfirst) {
    var slider = document.getElementById(target);
    var divs = slider.getElementsByTagName('div');
    var divslength = divs.length;
    for (i = 0; i < divslength; i++) {
        var div = divs[i];
        var divid = div.id;
        if (divid.indexOf("header") != -1) {
            div.onclick = new Function("processClick(this)");
        } else if (divid.indexOf("link") != -1) {
            var section = divid.replace('-link', '');
            array.push(section);
            div.maxh = div.offsetHeight;
            if (showfirst == 1 && i == 1) {
                div.style.display = 'block';
            } else {
                div.style.display = 'none';
            }
        }
    }
}

// Process the click - expand the selected content and collapse the others //
function processClick(div) {
    var catlength = array.length;
    for (i = 0; i < catlength; i++) {
        var section = array[i];
        var head = document.getElementById(section + '-header');
        var cont = section + '-link';
        var contdiv = document.getElementById(cont);
        clearInterval(contdiv.timer);
        if (head == div && contdiv.style.display == 'none') {
            contdiv.style.height = '0px';
            contdiv.style.display = 'block';
            initSlide(cont, 1);
        } else if (contdiv.style.display == 'block') {
            initSlide(cont, -1);
        }
    }
}

// Setup the variables and call the slide function //
function initSlide(id, dir) {
    var cont = document.getElementById(id);
    var maxh = cont.maxh;
    cont.direction = dir;
    cont.timer = setInterval("slide('" + id + "')", timer);
}

// Collapse or expand the div by incrementally changing the divs height and opacity //
function slide(id) {
    var cont = document.getElementById(id);
    var maxh = cont.maxh;
    var currheight = cont.offsetHeight;
    var dist;
    if (cont.direction == 1) {
        dist = (Math.round((maxh - currheight) / speed));
    } else {
        dist = (Math.round(currheight / speed));
    }
    if (dist <= 1) {
        dist = 1;
    }
    cont.style.height = currheight + (dist * cont.direction) + 'px';
    cont.style.opacity = currheight / cont.maxh;
    cont.style.filter = 'alpha(opacity=' + (currheight * 100 / cont.maxh) + ')';
    if (currheight < 2 && cont.direction != 1) {
        cont.style.display = 'none';
        clearInterval(cont.timer);
    } else if (currheight > (maxh - 2) && cont.direction == 1) {
        clearInterval(cont.timer);
    }
}