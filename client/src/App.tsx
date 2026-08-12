import { BrowserRouter, Route, Routes } from 'react-router-dom'
import Home from './pages/Home'
import Room from './pages/Room'
import './App.css'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/room/:code" element={<Room />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
