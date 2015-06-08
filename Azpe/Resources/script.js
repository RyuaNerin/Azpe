////////////////////////////////////////////////////////////
// 
// Azpe (based AZPreview made by @Gisuksagi)
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

	var vs = 'a110';

	var s = 1;
	var c = 2;
	var a = 4;

	System.launchApplication(path, vs + 'init', 1);

	System.addKeyBindingHandler('G'.charCodeAt(0), 0,
	function(id)
	{
		var item = TwitterService.status.get(id);

		if (!item) return;

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
			{
				var url = item.entities.urls[i++].expanded_url;
				if (!url.match('twitter\\.com/[a-zA-Z0-9_]+/status/\\d+'))
					arg += url + ',';
			}
		}

		if (arg.length > 1)
			System.launchApplication(path, vs + (item.retweeted ? item.retweeted_id : item.id) + ',show,' + arg.substring(0, arg.length - 1), 1);
	});

	System.addKeyBindingHandler('G'.charCodeAt(0), s,
	function(id)
	{
		var item = TwitterService.status.get(id);

		if (!item) return;

		System.launchApplication(path, vs + item.id + 'close', 1);
	});

	System.addKeyBindingHandler('G'.charCodeAt(0), a,
	function ()
	{
		var item = TwitterService.status.get(id);

		if (!item) return;

		System.launchApplication(path, vs + item.id + 'top', 1);
	});
}