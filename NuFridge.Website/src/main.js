import config from './authConfig';
export function configure(aurelia) {
  aurelia.use
    .standardConfiguration()
    .developmentLogging()
    .plugin('aurelia-animator-css')
    .plugin('paulvanbladel/aurelia-auth', (baseConfig)=>{
      baseConfig.configure(config);
    });

  aurelia.start().then(a => a.setRoot());
}
