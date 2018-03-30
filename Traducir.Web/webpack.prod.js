const merge = require('webpack-merge');
const webpack = require('webpack');
const UglifyJSPlugin = require('uglifyjs-webpack-plugin');

const common = require('./webpack.common.js');

module.exports = merge(common, {
    output: {
        filename: "[name].[chunkhash].bundle.js"
    },
    plugins: [
        new webpack.HashedModuleIdsPlugin(),
        new UglifyJSPlugin()
    ],
});