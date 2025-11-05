import axios from 'axios';

// Store conversation history per session
const userConversations = {}; // Example: { sessionId: [{ role: "user", content: "..." }, { role: "assistant", content: "..." }] }

// C# Recommendation Service URL
const RECOMMENDATION_SERVICE_URL = process.env.RECOMMENDATION_SERVICE_URL || 'http://localhost:5001';

export const getChatResponse = async (req, res) => {
    const { message, sessionId } = req.body;

    // Ensure sessionId is provided
    if (!sessionId) {
        return res.status(400).json({ error: "Session ID is required." });
    }

    // Initialize conversation history for the session if not already set
    if (!userConversations[sessionId]) {
        userConversations[sessionId] = [];
    }

    // Route ALL messages to C# recommendation service
    try {
        // Call C# recommendation microservice
        const recommendationResponse = await axios.post(
            `${RECOMMENDATION_SERVICE_URL}/api/recommendation/generate`,
            {
                message: message,
                sessionId: sessionId,
                conversationHistory: userConversations[sessionId] || []
            },
            {
                headers: {
                    'Content-Type': 'application/json'
                },
                timeout: 10000 // 10 second timeout
            }
        );

        if (recommendationResponse.data && recommendationResponse.data.reply) {
            const reply = recommendationResponse.data.reply;

            // Save the conversation context
            userConversations[sessionId].push({ role: "user", content: message });
            userConversations[sessionId].push({ role: "assistant", content: reply });

            return res.json({ reply });
        } else {
            throw new Error("Invalid response from recommendation service");
        }
    } catch (error) {
        console.error("Error calling recommendation service:", error.message);
        
        // Simple fallback when C# service is unavailable
        return res.json({ 
            reply: "Something went wrong, please try again." 
        });
    }
};
