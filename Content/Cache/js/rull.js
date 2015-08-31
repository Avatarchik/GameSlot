var sp=108;
var sp2;
var sp3;
var time1=450;
var flags1=0
$('.bg-stl1').click(function() {
	if(flags1==0) {
		flags1=1;
		rull();
	}
});
var j1=0;
var j2=0;
var j3;
var set2;
function mr(from, to)  {
	return Math.floor((Math.random() * (to - from + 1)) + from);
}
function rull() {
	$('.ov-elem1.j1').animate({left:-(sp*sp2)}, time1*sp2, 'linear');
	var set1=setInterval(function() {
		if(parseInt($('.ov-elem1.j1').css('left'))<-1) {
			if(j1==0) {
				$('.ov-elem1.j2').animate({right:-(sp*(sp2-10))}, time1*(sp2-10), 'linear');
				j1=1;
				setTimeout(function() {
					set2=setInterval(function() {
						$('.rr-chet1').text($('.ov-elem1.j1 .item1:not(.win1)').eq(mr(0,j3)).attr('dt'));
						rrch();
					}, time1*0.5);
				}, time1*3.8);
			}
		}
		if(parseInt($('.ov-elem1.j2').css('right'))<-1) {
			if(j2==0) {
				$('.ov-elem1.j3').animate({left:-(sp*(sp2-20))}, time1*(sp2-20), 'linear');
				j2=1;
			}
		}
		if(Math.abs(parseInt($('.ov-elem1.j2').css('right')))+116>sp3*sp) {
			clearInterval(set1);
			clearInterval(set2);
			$('.rr-chet1').text($('.ov-elem1.j1 .item1.win1').attr('dt'));
			rrch();
			$('.ov-elem1.j2').stop();
			$('.ov-elem1.j2').animate({right:-(sp3*sp+40-54)}, 600, 'linear');
			$('.ov-elem1.j2 .item1.win1').animate({marginLeft:'58px',marginRight:'58px'}, 600, 'linear');
			setTimeout(function() {
				$('.ov-elem1.j1').stop();
				$('.ov-elem1.j3').stop();
			},600);
			rull_done();
		}
	}, 1);
}
function addCommas(nStr)
{
	nStr += '';
	x = nStr.split('.');
	x1 = x[0];
	x2 = x.length > 1 ? '.' + x[1] : '';
	var rgx = /(\d+)(\d{3})/;
	while (rgx.test(x1)) {
		x1 = x1.replace(rgx, '$1' + ',' + '$2');
	}
	return x1 + x2;
}
function rrch() {
	$('.rr-chet1').each(function() {
		$(this).text(addCommas($(this).text()));
		$(this).html($(this).html().replace(/(\d)/g, '<span>$1</span>'));
		$(this).html($(this).html().replace(/(,)/g, '<i>$1</i>'));
	});
}
if($('.rr-chet1').length>0) {
	rrch();
}