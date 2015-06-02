////////////////////////////////////////////////////////////
// 
// AZPreivewE (based AZPreview made by @Gisuksagi)
// By RyuaNerin <admin@ryuanerin.kr>
//
////////////////////////////////////////////////////////////

if (System.apiLevel < 22)
{
	System.alert("아즈레아 업데이트가 필요합니다!\n업데이트 확인을 해주세요!");
}
else
{
	var path = System.applicationPath.replace(/[^\.\\]+\.exe/, '') + 'scripts\\azpe.js.Private\\azpe.exe';

	var vs = 'a100';

	var s = 1;
	var c = 2;
	var a = 4;

	System.launchApplication(path, vs + 'init', 0);

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
			System.launchApplication(path, vs + arg.substring(0, arg.length - 1), 0);

		else
			System.launchApplication(path, vs + 'focus', 0);
	});

	System.addKeyBindingHandler('G'.charCodeAt(0), s,
	function()
	{
		System.launchApplication(path, vs + 'hide', 0);
	});

	System.addKeyBindingHandler('G'.charCodeAt(0), a,
	function()
	{
		System.launchApplication(path, vs + 'top', 0);
	});
}