////////////////////////////////////////////////////////////
// 
// AZPreivewE (based AZPreview made by @Gisuksagi)
// By RyuaNerin <admin@ryuanerin.kr>
// 
// Last Update : 2015-05-15
//
//  |---------- S : Shift
//  ||--------- C : Ctrl
//  |||-------- A : Alt
//  |||
// [   ] + G : 이미지 보기
// [S  ] + G : 포커스
// [  A] + G : 맨 위로 설정/해제
// [SC ] + G : 종료
// [   ] + , : 이전 이미지
// [   ] + . : 다음 이미지
////////////////////////////////////////////////////////////

var path = System.applicationPath.replace(/[^(.)^(\\)]+(.)exe/, '') + 'Scripts\\AZPreviewE.js.Private\\AZPreviewE.exe';

var s = 1;
var c = 2;
var a = 4;

System.addKeyBindingHandler('G'.charCodeAt(0), 0,
function(id)
{
	var item = TwitterService.status.get(id);

	if (!item)
		return;

	var arg = "";

	if (item.entities.media.length > 0)
	{
		var i = 0;
		while (i < item.entities.media.length)
			arg += item.entities.media[i++].media_url + ',';
	}

	if (item.entities.urls.length > 0)
	{
		var i = 0;
		while (i < item.entities.urls.length)
			arg += item.entities.urls[i++].expanded_url + ',';
	}
	
	if (arg.length > 0)
		System.launchApplication(path, arg.substring(0, arg.length - 1), 0);

	else
		System.launchApplication(path, 'focus', 0);
});

System.addKeyBindingHandler('G'.charCodeAt(0), s,
function() {
	System.launchApplication(path, 'focus', 0);
});

System.addKeyBindingHandler('G'.charCodeAt(0), s | c,
function()
{
	System.launchApplication(path, 'exit', 0);
});

System.addKeyBindingHandler('G'.charCodeAt(0), a,
function()
{
	System.launchApplication(path, 'top', 0);
});

System.addKeyBindingHandler(','.charCodeAt(0), 0,
function()
{
	System.launchApplication(path, 'left', 0);
});

System.addKeyBindingHandler('.'.charCodeAt(0), 0,
function()
{
	System.launchApplication(path, 'right', 0);
});

CustomLib