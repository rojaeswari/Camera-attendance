import { Outlet } from "react-router-dom";
import Sidebar from "./Sidebar";

const Layout = () => {
    return (
        <div>
            <Sidebar />

            <main style={{ marginLeft: "240px" }}>
                <Outlet />
            </main>
        </div>
    );
};

export default Layout;