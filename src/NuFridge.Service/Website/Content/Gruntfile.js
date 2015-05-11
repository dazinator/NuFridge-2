/*global module, require */
module.exports = function(grunt) {
    'use strict';

    var mixIn = require('mout/object/mixIn');
    var requireConfig = {
        baseUrl: 'app/',
        paths: {
            'jquery': '../scripts/jquery-1.9.1',
            'knockout': '../scripts/knockout-3.1.0',
            'text': '../scripts/text',
            'durandal': '../scripts/durandal',
            'plugins': '../scripts/durandal/plugins',
            'transitions': '../scripts/durandal/transitions',
            'knockoutvalidation': '../Scripts/knockout.validation',
            'knockoutmapping': '../Scripts/knockout.mapping',
            'databinding-lucenefeed': 'viewmodels/databinding/lucenefeed',
            'databinding-lucenepackage': 'viewmodels/databinding/lucenepackage',
            'databinding-systeminfo': 'viewmodels/databinding/systeminfo',
            'databinding-user': 'viewmodels/databinding/user',
            'databinding-dashboard': 'viewmodels/databinding/dashboard',
            'readmore': '../Scripts/readmore'
        }
    };

    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        watch: {
            livereload: {
                files: ['app/**/*.js', 'app/**/*.html'],
                options: {
                    livereload: true
                }
            }
        },
        clean: {
            build: ['build/*']
        },
        copy: {
            lib: {
                src: 'scripts/**/**',
                dest: 'build/'
            },
            index: {
                src: 'index.html',
                dest: 'build/'
            },
            css: {
                src: 'Stylesheets/**',
                dest: 'build/'
            },
            font: {
                src: 'font/**',
                dest: 'build/'
            }
        },
        durandal: {
            release: {
                src: ['app/**/*.*', 'scripts/durandal/**/*.js'],
                options: {
                    name: '../scripts/almond-custom',
                    baseUrl: requireConfig.baseUrl,
                    mainPath: 'app/main',
                    paths: mixIn({}, requireConfig.paths, {
                        'almond': '../scripts/almond-custom.js'
                    }),
                    exclude: [],
                    optimize: 'none',
                    out: 'build/app/main.js'
                }
            },
            debug: {
                src: ['app/**/*.*', 'scripts/durandal/**/*.js'],
                options: {
                    name: '../scripts/almond-custom',
                    baseUrl: requireConfig.baseUrl,
                    mainPath: 'app/main',
                    paths: mixIn({}, requireConfig.paths, {
                        'almond': '../scripts/almond-custom.js'
                    }),
                    exclude: [],
                    optimize: 'none',
                    out: 'build/app/main.js'
                }
            }
        },
        jshint: {
            all: ['Gruntfile.js', 'app/**/*.js']
        },
        uglify: {
            options: {
                banner: '/*! <%= pkg.name %> <%= grunt.template.today("yyyy-mm-dd") %> \n' +
                    '* Copyright (c) <%= grunt.template.today("yyyy") %> NuFridge \n' +
                    '*/\n'
            },
            build: {
                src: 'build/app/main.js',
                dest: 'build/app/main-min.js'
            }
        },
    });

    // Loading plugin(s)
    grunt.loadNpmTasks('grunt-contrib-clean');
    grunt.loadNpmTasks('grunt-contrib-copy');
    grunt.loadNpmTasks('grunt-contrib-jshint');
    grunt.loadNpmTasks('grunt-contrib-uglify');
    grunt.loadNpmTasks('grunt-contrib-watch');
    grunt.loadNpmTasks('grunt-open');
    grunt.loadNpmTasks('grunt-durandal');

    grunt.registerTask('build release', ['jshint', 'clean', 'copy', 'durandal:release', 'uglify']);

};