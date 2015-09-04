import {inject} from 'aurelia-framework';
import {Router} from 'aurelia-router';
import AppRouterConfig from 'app.router.config';
import HttpClientConfig from 'paulvanbladel/aurelia-auth/app.httpClient.config';
import 'jquery';
import 'semanticui/semantic';

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
  }
}