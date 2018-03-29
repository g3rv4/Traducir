import * as React from 'react'
import * as ReactDOM from "react-dom";
import { hot } from 'react-hot-loader'
import Traducir from './App/Traducir'

const TraducirHot = hot(module)(Traducir)

ReactDOM.render(
  <TraducirHot />,
  document.getElementById("root")
);