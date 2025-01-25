import express from 'express';
import { getChatResponse } from '../controllers/chatController.js';

const router = express.Router();
// Define the chatbot route
router.post('/', getChatResponse);

export default router;
