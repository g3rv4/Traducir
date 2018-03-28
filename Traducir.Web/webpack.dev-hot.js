const merge = require('webpack-merge');
const dev = require('./webpack.dev.js');
const webpack = require('webpack');
const _ = require('lodash');

let merged = merge(dev, {
    entry: {
        index: './src/index-hot.tsx',
    },
    devServer: {
        hot: true
    },
    plugins: [
        new webpack.NamedModulesPlugin()
    ]
});

merged.entry.vendor.push('react-hot-loader');

module.exports = merged;