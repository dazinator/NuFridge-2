var gulp = require('gulp');
var browserSync = require('browser-sync');

// this task utilizes the browsersync plugin
// to create a dev server instance
// at http://localhost:8081
gulp.task('serve', ['build'], function(done) {
  browserSync({
    open: false,
    port: 8081,
	proxy: "localhost:8080",
	notify: true
  }, done);
});