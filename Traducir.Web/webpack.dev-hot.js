const merge = require('webpack-merge');
const common = require('./webpack.common.js');
const webpack = require('webpack');
const _ = require('lodash');

let merged = merge(common, {
    mode: "development",
    entry: {
        index: './src/index-hot.tsx',
    },
    devServer: {
        hot: true,
        historyApiFallback: true,
        disableHostCheck: true,
        proxy: {
            "/app": "http://localhost:5000"
        }
    },
    plugins: [
        new webpack.NamedModulesPlugin()
    ]
});

merged.entry.vendor.push('react-hot-loader');

module.exports = merged;
