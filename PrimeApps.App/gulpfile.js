var fs = require('fs');
var path = require('path');
var gulp = require('gulp');
var uglify = require('gulp-uglify');
var minify = require('gulp-minify-css');
var stripDebug = require('gulp-strip-debug');
var runSequence = require('run-sequence');
var bundle = require('gulp-bundle-assets');

gulp.task('build', function (callback) {
    runSequence(
        'strip',
        ['uglify', 'minify', 'bundle'],
        callback);
});

gulp.task('strip', function () {
    //Striping console and debugger statements from views directory
    var directories = getDirectories('views/');

    for (var i = 0, l = directories.length; i < l; i++) {
        gulp.src('views/' + directories[i] + '/*.js')
            .pipe(stripDebug())
            .pipe(gulp.dest('views/' + directories[i] + '/'));

        var subDirectories = getDirectories('views/' + directories[i] + '/');

        for (var j = 0, lj = subDirectories.length; j < lj; j++) {
            gulp.src('views/' + directories[i] + '/' + subDirectories[j] + '/*.js')
                .pipe(stripDebug())
                .pipe(gulp.dest('views/' + directories[i] + '/' + subDirectories[j] + '/'));

            var subSubDirectories = getDirectories('views/' + directories[i] + '/' + subDirectories[j] + '/');

            for (var k = 0, jk = subSubDirectories.length; k < jk; k++) {
                gulp.src('views/' + directories[i] + '/' + subDirectories[j] + '/' + subSubDirectories[k] + '/*.js')
                    .pipe(stripDebug())
                    .pipe(gulp.dest('views/' + directories[i] + '/' + subDirectories[j] + '/' + subSubDirectories[k] + '/'));

                var subSubSubDirectories = getDirectories('views/' + directories[i] + '/' + subDirectories[j]+ '/'  + subSubDirectories[k] + '/');

                for (var m = 0, km = subSubSubDirectories.length; m < km; m++) {
                    gulp.src('views/' + directories[i] + '/' + subDirectories[j]+ '/'  + subSubDirectories[k] + '/' + subSubSubDirectories[m] + '/*.js')
                        .pipe(stripDebug())
                        .pipe(gulp.dest('views/' + directories[i] + '/' + subDirectories[j]+ '/'  + subSubDirectories[k] + '/' +  subSubSubDirectories[m] + '/'));
                }
            }
        }
    }

    //Striping console and debugger statements from scripts directory
    return gulp.src('scripts/*.js')
        .pipe(stripDebug())
        .pipe(gulp.dest('scripts/'));
});

gulp.task('uglify', function () {
    //Uglifying views directory
    var directories = getDirectories('views/');

    for (var i = 0, l = directories.length; i < l; i++) {
        gulp.src('views/' + directories[i] + '/*.js')
            .pipe(uglify())
            .pipe(gulp.dest('views/' + directories[i] + '/'));

        var subDirectories = getDirectories('views/' + directories[i] + '/');

        for (var j = 0, lj = subDirectories.length; j < lj; j++) {
            gulp.src('views/' + directories[i] + '/' + subDirectories[j] + '/*.js')
                .pipe(uglify())
                .pipe(gulp.dest('views/' + directories[i] + '/' + subDirectories[j] + '/'));

            var subSubDirectories = getDirectories('views/' + directories[i] + '/' + subDirectories[j] + '/');

            for (var k = 0, jk = subSubDirectories.length; k < jk; k++) {
                gulp.src('views/' + directories[i] + '/' + subDirectories[j] + '/' + subSubDirectories[k] + '/*.js')
                    .pipe(uglify())
                    .pipe(gulp.dest('views/' + directories[i] + '/' + subDirectories[j] + '/' + subSubDirectories[k] + '/'));

                var subSubSubDirectories = getDirectories('views/' + directories[i] + '/' + subDirectories[j]+ '/'  + subSubDirectories[k] + '/');

                for (var m = 0, km = subSubSubDirectories.length; m < km; m++) {
                    gulp.src('views/' + directories[i] + '/' + subDirectories[j]+ '/'  + subSubDirectories[k] + '/' + subSubSubDirectories[m] + '/*.js')
                        .pipe(uglify())
                        .pipe(gulp.dest('views/' + directories[i] + '/' + subDirectories[j]+ '/'  + subSubDirectories[k] + '/' +  subSubSubDirectories[m] + '/'));
                }
            }
        }
    }

    //Uglifying scripts directory
    return gulp.src('scripts/*.js')
        .pipe(uglify())
        .pipe(gulp.dest('scripts/'));
});

gulp.task('minify', function () {
    return gulp.src('styles/*.css')
        .pipe(minify({keepSpecialComments: false}))
        .pipe(gulp.dest('styles/'));
});

gulp.task('bundle', function() {
    return gulp.src('./bundle.config.js')
        .pipe(bundle())
        .pipe(gulp.dest('./dist'));
});

//Helpers
function getDirectories(srcpath) {
    return fs.readdirSync(srcpath).filter(function (file) {
        return fs.statSync(path.join(srcpath, file)).isDirectory();
    });
}