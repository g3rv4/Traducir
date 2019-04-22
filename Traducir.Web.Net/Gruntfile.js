module.exports = function (grunt) {
    grunt.initConfig({
        ts: {
            default: {
                tsconfig: './tsconfig.json'
            }
        },
        copy: {
            js: {
                files: [
                    { expand: true, src: ["**/*.js"], cwd: "Scripts/", dest: "wwwroot/js/" },
                ],
            }
        },
        clean: {
            all: {
                src: ["wwwroot/js", "wwwroot/dist"]
            }
        },
        watch: {
            tsfiles: {
                files: "Scripts/**/*.ts",
                tasks: ["tslint:dev", "ts", "rollup"],
                options: {
                    spawn: false,
                }
            },
            js: {
                files: "Scripts/**/*.js",
                tasks: ["copy"]
            }
        },
        tslint: {
            dev: {
                files: {
                    src: "Scripts/**/*.ts"
                }
            },
            prod: {
                options: {
                    configuration: "tslint.prod.json"
                },
                files: {
                    src: "Scripts/**/*.ts"
                }
            }
        },
        rollup: {
            options: {
                format: 'iife'
            },
            strings: {
                src: 'wwwroot/dist/modules/strings/strings.js',
                dest: 'wwwroot/js/strings.js'
            },
            users: {
                src: 'wwwroot/dist/modules/users/user-edit.js',
                dest: 'wwwroot/js/users.js'
            }
        }
    });
    grunt.loadNpmTasks("grunt-ts");
    grunt.loadNpmTasks("grunt-rollup");
    grunt.loadNpmTasks("grunt-tslint");
    grunt.loadNpmTasks("grunt-contrib-watch");
    grunt.loadNpmTasks("grunt-contrib-copy");
    grunt.loadNpmTasks("grunt-contrib-clean");

    grunt.registerTask("default", ["tslint:dev", "clean", "ts", "rollup", "copy"]);
    grunt.registerTask("watch", ["tslint:dev", "clean", "ts", "rollup", "copy", "watch"]);

    grunt.registerTask("generate-tslint-prod", "Generate the tslint file for prod", function () {
        let tslint = grunt.file.readJSON("tslint.json");

        delete tslint.rules["no-debugger"];
        delete tslint.rules["no-console"];

        grunt.file.write("tslint.prod.json", JSON.stringify(tslint, null, 2));
    });
};
