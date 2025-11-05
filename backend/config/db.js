import mongoose from "mongoose";

export const connectDB = async () => {
    const uri = process.env.MONGODB_URI || 'mongodb+srv://abhinav:Abhi%408520@cluster0.dnfuj.mongodb.net/food-del?retryWrites=true&w=majority&appName=Cluster0'
    try {
        await mongoose.connect(uri, {
            serverSelectionTimeoutMS: 5000,
        });
        console.log("DB Connected");
    } catch (err) {
        console.error("DB Connection Error:", err.message);
    }
};
