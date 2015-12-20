var gulp = require('gulp');  
var bundle = require('aurelia-bundler').bundle;

var config = {  
  force: true,
  packagePath: '.',
  bundles: {
    "dist/app-build": {
      includes: [
        '*',
        '*.html!text',
        '*.css!text',
      ],
      options: {
        inject: true,
        minify: true
      }
    },
    "dist/aurelia": {
      includes: [
        'aurelia-bootstrapper',
        'aurelia-fetch-client',
        'aurelia-router',
        'aurelia-animator-css'
      ],
      options: {
        inject: true,
        minify: true
      }
    }
  }
};

gulp.task('bundle', function() {  
  return bundle(config);
});