import * as React from 'react'
import * as ReactDOM from "react-dom";
import { hot } from 'react-hot-loader'
import { BrowserRouter } from 'react-router-dom'
import Traducir from './App/Traducir'

const TraducirHot = hot(module)(Traducir)

ReactDOM.render(
  <BrowserRouter>
    <TraducirHot />
  </BrowserRouter>,
  document.getElementById("root")
);