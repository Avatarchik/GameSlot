/*
<span class='action'><span class='action-date' textafter='' year='2014' month='12' day='31'></span></span>
<span class="actionday"></span>
<span class="actionhour"></span>
<span class="actionmin"></span>
<span class="actionsec"></span>
*/
var montharray=new Array("Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec");

function CountDowndmn(yr,m,d){
	cdyear=yr;
	cdmonth=m;
	cdday=d;
	var today=new Date();
	var todayy=today.getYear();
	if (todayy < 1000)
	todayy+=1900;
	var todaym=today.getMonth();
	var todayd=today.getDate();
	var todayh=today.getHours();
	var todaymin=today.getMinutes();
	var todaysec=today.getSeconds();
	var todaystring=montharray[todaym]+" "+todayd+", "+todayy+" "+todayh+":"+todaymin+":"+todaysec;
	futurestring=montharray[m-1]+" "+d+", "+yr
	dd=Date.parse(futurestring)-Date.parse(todaystring);
	dday=Math.floor(dd/(60*60*1000*24)*1);
	dhour=Math.floor((dd%(60*60*1000*24))/(60*60*1000)*1);
	dmin=Math.floor(((dd%(60*60*1000*24))%(60*60*1000))/(60*1000)*1);
	dsec=Math.floor((((dd%(60*60*1000*24))%(60*60*1000))%(60*1000))/1000*1);
    if(dday<=0&&dhour<=0&&dmin<=0&&dsec<=1){
return 'end';
}
else {
	var lastchar = ""+dsec;	lastchar = lastchar.substring(lastchar.length-1,lastchar.length);
	var dsecstr = "<small>���.</small>";
	
	lastchar = ""+dmin;	lastchar = lastchar.substring(lastchar.length-1,lastchar.length);
	var dminstr    = "<small>�. </small>";

	lastchar = ""+dhour;	lastchar = lastchar.substring(lastchar.length-1,lastchar.length);
	var dhourstr   = "<small>�. </small>";

	lastchar = ""+dday;	lastchar = lastchar.substring(lastchar.length-1,lastchar.length);
	var ddaystr = "<small>��. </small>";

	//return ""+dday+ "" +ddaystr+""+dhour+""+dhourstr+""+dmin+""+dminstr+""+dsec+""+dsecstr;

        var res = new Array();

        if(dday<10) res[0] = '0'+dday; else res[0] = dday;
        if(dhour<10) res[1] = '0'+dhour; else res[1] = dhour;
        if(dmin<10) res[2] = '0'+dmin; else res[2] = dmin;
        if(dsec<10) res[3] = '0'+dsec; else res[3] = dsec;

        return res;  
}
}

$(document).ready(function() {
   setTimeout("actionsUpdate()",100); 
});

function actionsUpdate() {
   $('.action').each(
      function(){
         var obj=$(this).find('.action-date');
         var year=$(obj).attr('year');
         var month=$(obj).attr('month');
         var day=$(obj).attr('day');
         var textafter=$(obj).attr('textafter');

         //var text = CountDowndmn(year,month,day); 
         //if(text=='end') {text=textafter; $(this).find('.act-txt').hide();}
         //$(obj).html(text);

         var res = CountDowndmn(year,month,day);
         $('.actionday').html(res[0]);
         $('.actionhour').html(res[1]);
         $('.actionmin').html(res[2]);
         $('.actionsec').html(res[3]);


      }
   )
   $('.action1').each(
      function(){
         var obj=$(this).find('.action-date');
         var year=$(obj).attr('year');
         var month=$(obj).attr('month');
         var day=$(obj).attr('day');
         var textafter=$(obj).attr('textafter');

         //var text = CountDowndmn(year,month,day); 
         //if(text=='end') {text=textafter; $(this).find('.act-txt').hide();}
         //$(obj).html(text);

         var res = CountDowndmn(year,month,day);
         $('.actionday1').html(res[0]);
         $('.actionhour1').html(res[1]);
         $('.actionmin1').html(res[2]);
         $('.actionsec1').html(res[3]);


      }
   )
   setTimeout("actionsUpdate()",1000);
}