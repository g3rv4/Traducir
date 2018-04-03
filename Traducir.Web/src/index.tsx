import * as React from "react";
import * as ReactDOM from "react-dom";
import { BrowserRouter } from 'react-router-dom'

import Traducir from "./App/Traducir";

ReactDOM.render(
  <BrowserRouter>
    <Traducir />
  </BrowserRouter>,
  document.getElementById("root")
);