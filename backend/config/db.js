import mongoose from "mongoose";

export const connectDB = async () => {
    // Always use environment variable - no hardcoded credentials
    const uri = process.env.MONGODB_URI;
    
    if (!uri) {
        console.error("Error: MONGODB_URI environment variable is not set. Please configure it in your .env file.");
        process.exit(1);
    }
    
    try {
        await mongoose.connect(uri, {
            serverSelectionTimeoutMS: 5000,
        });
        console.log("DB Connected");
    } catch (err) {
        console.error("DB Connection Error:", err.message);
        process.exit(1);
    }
};
