import mongoose from "mongoose";

export const connectDB = async () => {
    try {
        await mongoose.connect('mongodb+srv://abhinav:Abhi%408520@cluster0.dnfuj.mongodb.net/food-del?retryWrites=true&w=majority&appName=Cluster0', {
        });
        console.log("DB Connected");
    } catch (err) {
        console.error("DB Connection Error:", err.message);
    }
};
