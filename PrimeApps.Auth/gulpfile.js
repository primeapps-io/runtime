var fs = require('fs');
var path = require('path');
var gulp = require('gulp');
var uglify = require('gulp-uglify');
var minify = require('gulp-clean-css');
var stripDebug = require('gulp-strip-debug');
var runSequence = require('run-sequence');
var bundle = require('gulp-bundle-assets');

gulp.task('build', function (callback) {
	runSequence(
		['minify', 'bundle'],
		callback);
});

gulp.task('minify', function () {
	return gulp.src('wwwroot/styles/*.css')
		.pipe(minify({ compatibility: 'ie8' }))
		.pipe(gulp.dest('wwwroot/styles/'));
});

gulp.task('bundle', function () {
	return gulp.src('./bundle.config.js')
		.pipe(bundle())
		.pipe(gulp.dest('./wwwroot/bundles'));
});
