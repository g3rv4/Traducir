const HtmlWebpackPlugin = require('html-webpack-plugin');
const webpack = require('webpack');
const CleanWebpackPlugin = require('clean-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const TsCheckerWebpackPlugin = require("ts-checker-webpack-plugin");
const path = require("path");

module.exports = {
    devtool: "source-map",
    entry: {
        index: './src/index.tsx',
        vendor: [
            'react',
            'react-dom',
            'axios',
            'lodash'
        ]
    },
    output: {
        filename: "[name].bundle.js",
        path: __dirname + "/dist",
        publicPath: '/'
    },

    resolve: {
        extensions: [".ts", ".tsx", ".js", ".json"]
    },

    plugins: [
        new HtmlWebpackPlugin({
            template: "./src/index.ejs",
            chunks: ['vendor', 'index']
        }),
        new CopyWebpackPlugin([
            { from: 'lib', to: 'lib/' }
        ]),
        new TsCheckerWebpackPlugin({
            tsconfig: path.resolve("tsconfig.json"),
            tslint: path.resolve("tslint.json"), // optional
            memoryLimit: 512, // optional, maximum memory usage in MB
            diagnosticFormatter: "ts-loader", // optional, one of "ts-loader", "stylish", "codeframe"
        })
    ],

    module: {
        rules: [
            // All files with a '.ts' or '.tsx' extension will be handled by 'awesome-typescript-loader'.
            {
                test: /\.tsx?$/,
                use: [{
                    loader: 'babel-loader'
                }, {
                    loader: 'ts-loader',
                    options: {
                        transpileOnly: true
                    }
                }]
            },

            // All output '.js' files will have any sourcemaps re-processed by 'source-map-loader'.
            {
                enforce: "pre",
                test: /\.js$/,
                loader: "source-map-loader"
            }
        ]
    }
};
