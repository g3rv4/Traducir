import * as React from 'react'
import * as ReactDOM from "react-dom";
import { hot } from 'react-hot-loader'
import { Router } from 'react-router-dom'
import history from './history'
import Traducir from './App/Traducir'

const TraducirHot = hot(module)(Traducir)

ReactDOM.render(
  <Router history={history}>
    <TraducirHot />
  </Router>,
  document.getElementById("root")
);