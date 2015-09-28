$(document).ready(function() {
	$('input, textarea').placeholder();
	/* PIE */
	if (window.PIE) {
		$('nav').each(function() {
			PIE.attach(this);
		});
	}
	var ff;
	/*$('input[type=text]:not(.datapick)').focus(function() {
		if($(this).attr('data-place')==$(this).val()) {
			$(this).val('');
		}
	});
	$('input[type=text]:not(.datapick)').blur(function() {
		if($(this).val()=='') {
			$(this).val($(this).attr('data-place'));
		}
	});
	$('textarea').focus(function() {
		if($(this).attr('data-place')==$(this).val()) {
			$(this).val('');
		}
	});
	$('textarea').blur(function() {
		ff=$(this).attr('data-place');
		if($(this).val().length==0) {
			$(this).val(ff);
		}
	});*/
	function ress() {
		if($('.dc-f1').length>0) {
			$('.dc-f1').each(function() {
				$(this).height($(this).parent().height());
			});
		}
		$('.modal-tb').width($(window).width()).height($(window).height());
		$('.bf-tt2').css({'min-height':$(window).height()});
		$('.bf-tt3').height($('#page').height()).css({'min-height':$(window).height()});
		if($('.bf-tt4').length>0) {
			$('.bf-tt4').height($('#content').offset().top);
		}
	}
	ress();
	if($('#content').length>0) {
		$('#content').before('<div class="poss-content1"></div>')
	}
	$(window).load(function() {
		$('input[type=text]').each(function() {
			$(this).attr('data-place',$(this).val());
		});
		if($('#header3').length>0) {
			if($('#header2').length==0) {
				$('#header3').addClass('nohead')
			}
		}
		$('.cont-ov-items-more1 .item1,.ov-ic-more-s1 .item1,.over-items-more-bl1 .rr1 ,item1,.owl1 .item').each(function() {
			$(this).find('.tiltp').css({'margin-left':-($(this).find('.tiltp').width()*0.5+10)})
		});
		if($('.content-ct-item1').length>0) {
			$('.content-ct-item1 .cont-ov-items-more1').each(function() {
				$(this).find('.item1').eq(0).find('.tiltp').addClass('j3');
				$(this).find('.item1').eq(1).find('.tiltp').addClass('j3');
				$(this).find('.item1').eq(2).find('.tiltp').addClass('j3');
				$(this).find('.item1').eq(3).find('.tiltp').addClass('j3');
				$(this).find('.item1').eq(4).find('.tiltp').addClass('j3');
				$(this).find('.item1').eq(5).find('.tiltp').addClass('j3');
			});
		}
		$('textarea').each(function() {
			$(this).attr('data-place',$(this).val());
		});
		if($(".owl1").length>0&&5==3) {
			var owl = $(".owl1");
			owl.owlCarousel({
				items : 7
			});
			$(".nn1").click(function(){
				owl.trigger('owl.next');
			});
			$(".pp1").click(function(){
				owl.trigger('owl.prev');
			});
		}
		if($('.ov-elem1').length>0) {

		}
		ress();
	});
	$(window).resize(function() {
		ress();
	});
	$('.tb-s1-table a').toggle(function(e) {
		e.preventDefault();
		$(this).addClass('active');
	},function() {
		$(this).removeClass('active');
	});
	$('.tb-head1 a').toggle(function(e) {
		e.preventDefault();
		$(this).addClass('active');
	},function() {
		$(this).removeClass('active');
	});
	$('.ll-head1 a').toggle(function(e) {
		e.preventDefault();
		$(this).addClass('active');
	},function() {
		$(this).removeClass('active');
	});
	$('.et-q1').click(function() {
		if($(this).attr('dt')==0) {
			$(this).parent().parent().siblings().find('.et-q1').attr('dt','0').next().slideUp(200);
			$(this).attr('dt','1').next().slideDown(200);
		}
		else {
			$(this).attr('dt','0').next().slideUp(200);
		}
	});
	
if($(".cont-ll1").length>0) {
	$(".cont-ll1").mCustomScrollbar({
		horizontalScroll:false,
		mouseWheelPixels: 300
	});
}
$('.nav-chat1 li a').click(function(e) {
	e.preventDefault();
	$(this).parent().addClass('active').siblings().removeClass('active');
	$('.ev-item-et1').eq($(this).parent().index()).addClass('active').siblings().removeClass('active');
});
$('.tit-chat1').click(function(e) {
	e.preventDefault();
	if($('.chat-fix1').attr('dt')=='1') {
		$('.chat-fix1').attr('dt','0').removeClass('active');
	}
	else {
		$('.chat-fix1').attr('dt','1').addClass('active');
	}
});
if($(".content1").length>0) {
	$(".content1").mCustomScrollbar({
		horizontalScroll:false,
		mouseWheelPixels: 300
	});
}
if($(".content3").length>0) {
	$(".content3").mCustomScrollbar({
		horizontalScroll:false,
		mouseWheelPixels: 300
	});
}
if($(".content-modal1").length>0) {
	$(".content-modal1").mCustomScrollbar({
		horizontalScroll:false,
		mouseWheelPixels: 300
	});
}
if($(".content-ct-item1").length>0) {
	$(".content-ct-item1").mCustomScrollbar({
		horizontalScroll:false,
		mouseWheelPixels: 300
	});
}
$('.sl-link1').click(function(e) {
	e.preventDefault();
	$(this).parent().parent().addClass('active');
});
$('.back1').click(function(e) {
	e.preventDefault();
	$(this).parent().parent().removeClass('active');
});
$('.bg-modal,.close1').click(function() {
	$('.modal').fadeOut();
});
if($('.sort-chk1').length>0) {
	$('.sort-chk1 input').styler();
}
if($('.enter-pay2').length>0) {
	$('.enter-pay2 input').styler();
}
if($('.mt2-store1').length>0) {
	$('.mt2-store1 select').styler();
}
if($('.lv-check1').length>0) {
	$('.lv-check1 input').styler();
}
if($('.jcheck').length>0) {
	$('.jcheck input').styler();
}
$('.sort-chk1 label:first-child').change(function() {
	if($(this).find('input:checked').length>0) {
		$(this).parent().parent().next().find('input').attr('checked','checked').trigger('refresh');
	}
	else {
		$(this).parent().parent().next().find('input').removeAttr('checked').trigger('refresh');
	}
});
$('.klv1 input').keydown(function(e) {
	if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
             (e.keyCode == 65 && e.ctrlKey === true) || 
             (e.keyCode >= 35 && e.keyCode <= 39)) {
             return;
         }
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
        	e.preventDefault();
        }
    });
$('.klv1 span:first-child').click(function() {
	if(parseInt($(this).parent().prev().val())==0) {

	}
	else {
		$(this).parent().prev().val(parseInt($(this).parent().prev().val())-1);
	}
});
$('.klv1 span:last-child').click(function() {
	$(this).parent().prev().val(parseInt($(this).parent().prev().val())+1);
	if($(this).parent().parent().next().attr('dt')=='max') {
		if(parseInt($(this).parent().prev().val())>parseInt($(this).parent().parent().next().find('span').text())) {
			$(this).parent().prev().val(parseInt($(this).parent().parent().next().find('span').text()));
		}
	}
});
$('.form-hide1 input[type=submit').click(function(e) {
	e.preventDefault();
	$('.form1').addClass('active');
});
$('.and-more1').click(function(e) {
	e.preventDefault();
	$('.form1').removeClass('active');
});
$('.refresh-inv1').click(function(e) {
	e.preventDefault();
	$(this).addClass('active');
	setTimeout(function() {
		$('.refresh-inv1').removeClass('active');
	}, 2000);
});
$('.up1').click(function() {
	$('body,html').animate({scrollTop:'0px'}, 500)
});
$(window).scroll(function() {
	if($(this).scrollTop()>500) {
		$('.up1').fadeIn(300);
	}
	else {
		$('.up1').fadeOut(300);
	}
});
setTimeout(function() {
	$('.eft-pl1.j1').text('123123').addClass('active').delay(800).animate({opacity:'0',marginTop:'-20px'}, 1500);
	$('.eft-pl1.j2').text('123123').addClass('active').delay(800).animate({opacity:'0',marginTop:'-20px'}, 1500);
	$('.eft-pl1.j3').text('123123').addClass('active').delay(800).animate({opacity:'0',marginTop:'-20px'}, 1500);
},3000);
$('.ft-nav1 a').click(function(e) {
	e.preventDefault();
	$(this).parent().parent().parent().next().find('.over-tt-item1').eq($(this).parent().index()).addClass('active').siblings().removeClass('active');
	$(this).parent().addClass('active').siblings().removeClass('active');
});
if($('.avavavava1').length>0) {

}
else {
	$('.sp-tt1').live('click',function(e) {
		e.preventDefault();
		if($(this).attr('dt')==0) {
			$('.evr1').parent().removeClass('active').find('.sp-tt1:not(.diss1)').attr('dt','0');
			$(this).parent().addClass('active');
			$(this).attr('dt','1');
		}
		else if($(this).attr('dt')==2) {

		}
		else {
			$(this).attr('dt','0');
			$(this).parent().removeClass('active');
		}
	});
}
$(document).click(function(e){
	if ($(e.target).closest(".bl-st2").length) return;
	$('.bl-st2').removeClass('active').find('.sp-tt1:not(.diss1)').attr('dt','0');
	e.stopPropagation();
});
$('.content3 .item1 a,.evr1 a').click(function(e) {
	e.preventDefault();
});
if($("#slider-range").length>0) {
	accounting.settings = {
	                currency: {
	                    symbol : "",  
	                    format: "%s%v",
	                    decimal : " ",  
	                    thousand: " ",  
	                },
	                number: {
	                    precision : 0,  
	                    thousand: ".",
	                    decimal : "."
	                }
	            }
	$("#slider-range").slider({
	      range: true,
	      min: 0,
	      max: 1560,
	      values: [0,1560],
	      slide: function(event,ui) {
	        $('.numbs-double1 div').eq(0).text(accounting.formatMoney(ui.values[0]));
	        $('.numbs-double1 div').eq(1).text(accounting.formatMoney(ui.values[1]));
	        $('.ip-slider1 input').eq(0).val(accounting.formatMoney(ui.values[0]));
	        $('.ip-slider1 input').eq(1).val(accounting.formatMoney(ui.values[1]));
	      }
    });
    $('.numbs-double1 div').eq(0).text(accounting.formatMoney($("#slider-range").slider("values",0)));
    $('.numbs-double1 div').eq(1).text(accounting.formatMoney($("#slider-range").slider("values",1)));
    $('.ip-slider1 input').eq(0).val(accounting.formatMoney($("#slider-range").slider("values",0)));
    $('.ip-slider1 input').eq(1).val(accounting.formatMoney($("#slider-range").slider("values",1)));
    $('.ip-slider1 input').eq(0).blur(function() {
    	$('.numbs-double1 div').eq(0).text(accounting.formatMoney($(this).val().replace(/\s/g, '')));
    	$("#slider-range").slider("values",0,$(this).val().replace(/\s/g, ''));
    	$(this).val(accounting.formatMoney($(this).val()));
    });
    $('.ip-slider1 input').eq(1).blur(function() {
    	$('.numbs-double1 div').eq(1).text(accounting.formatMoney($(this).val().replace(/\s/g, '')));
    	$("#slider-range").slider("values",1,$(this).val().replace(/\s/g, ''));
    	$(this).val(accounting.formatMoney($(this).val()));
    });
}
$('.ip-slider1 input').keydown(function(e) {
		if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||

             (e.keyCode == 65 && e.ctrlKey === true) || 
             (e.keyCode >= 35 && e.keyCode <= 39)) {
             return;
         }
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
        	e.preventDefault();
        }
    });
$('.ct-t1 .rr1 .active a').click(function(e) {
	e.preventDefault();
});
$('.frm-ord-now1').click(function(e) {
	e.preventDefault();
	$(this).hide().next().show();
});
if($(".date-ip1").length>0) {
	$(".date-ip1 input").datepicker();
}
/*$('.show-more-items1').click(function(e) {
	e.preventDefault();
	$('.content3.j1').mCustomScrollbar({
		horizontalScroll:false,
		mouseWheelPixels: 300
	});
	alert('Окошко алерта типо загрузилось')
});*/
});