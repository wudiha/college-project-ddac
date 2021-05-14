var gulp = require('gulp'),
    saas = require('gulp-sass');

gulp.task('build-css', function () {

    return gulp
        .src('./SASS/**/*.scss')
        .pipe(saas())
        .pipe(gulp.dest('./CSS'));

});
gulp.task('default', ['build-css']);