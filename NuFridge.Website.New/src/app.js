import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import AppRouterConfig from 'app.router.config';
import HttpClientConfig from 'aurelia-auth/app.httpClient.config';
import 'jquery';
import 'semanticui/semantic';
import 'semanticui/semantic.css!';
import "/styles/custom.css!";

@inject(Router,HttpClientConfig,AppRouterConfig)
export class App {

  constructor(router, httpClientConfig, appRouterConfig){
    this.router = router;
    this.httpClientConfig = httpClientConfig;
    this.appRouterConfig = appRouterConfig;
  }

  activate(){

    this.httpClientConfig.configure();
    this.appRouterConfig.configure();

    (function($) {
        $.QueryString = (function(a) {
            if (a == "") return {};
            var b = {};
            for (var i = 0; i < a.length; ++i)
            {
                var p=a[i].split('=');
                if (p.length != 2) continue;
                b[p[0]] = decodeURIComponent(p[1].replace(/\+/g, " "));
            }
            return b;
        })(window.location.search.substr(1).split('&'))
    })(jQuery);
  }
}
