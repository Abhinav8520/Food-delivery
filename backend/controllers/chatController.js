import OpenAI from 'openai';
import menu from '../data/menu.json' assert { type: "json" };

const openai = new OpenAI({
    apiKey: process.env.OPENAI_API_KEY, 
});

// Store conversation history per session
const userConversations = {}; // Example: { sessionId: [{ role: "user", content: "..." }, { role: "assistant", content: "..." }] }

const faqResponses = {
    "what is your name": "My name is Ziggy, your friendly assistant.",
    "what services do you provide": "I can help you with food recommendations and general queries.",
    "how can i track my order": "You can track your order by visiting the 'My Orders' section in your account.",
};

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

    // Normalize the input for case-insensitive and punctuation-free matching
    const normalizedMessage = message.toLowerCase().replace(/[?.!]/g, "").trim();

    // 1. Check for FAQs
    if (faqResponses[normalizedMessage]) {
        const reply = faqResponses[normalizedMessage];

        // Save the conversation context
        userConversations[sessionId].push({ role: "user", content: message });
        userConversations[sessionId].push({ role: "assistant", content: reply });

        return res.json({ reply });
    }

    // 2. Check for menu recommendations
    if (normalizedMessage.includes("recommend")|| normalizedMessage.includes("suggest")) {
        try {
            let recommendations = [];

            // Dynamically filter based on keywords
            if (normalizedMessage.includes("low spice")) {
                recommendations = menu.filter(item => item.spiceLevel === "low" && item.availability);
            } else if (normalizedMessage.includes("vegetarian")) {
                recommendations = menu.filter(item => item.tags.includes("vegetarian") && item.availability);
            } else if (normalizedMessage.includes("dessert") || normalizedMessage.includes("sweet")) {
                recommendations = menu.filter(item => item.category.toLowerCase() === "desserts" || item.category.toLowerCase() === "cake");
            } else {
                recommendations = menu.filter(item => item.availability); // Default to all available items
            }

            if (recommendations.length > 0) {
                const responseText = recommendations
                    .map(item => `${item.name}: ${item.description} (Price: $${item.price}, Rating: ${item.rating}/5)`)
                    .join("\n");

                // Save the conversation context
                userConversations[sessionId].push({ role: "user", content: message });
                userConversations[sessionId].push({ role: "assistant", content: responseText });

                return res.json({ reply: `Here are some recommendations:\n${responseText}` });
            } else {
                return res.json({ reply: "I couldn't find any items matching your request. Please try a different query." });
            }
        } catch (error) {
            console.error("Error querying menu:", error);
            return res.status(500).json({ error: "Error fetching menu recommendations." });
        }
    }

    // 3. Default OpenAI API response for other queries
    try {
        // Append user's message to the conversation history
        userConversations[sessionId].push({ role: "user", content: message });

        // Send the conversation history to OpenAI
        const response = await openai.chat.completions.create({
            model: "gpt-3.5-turbo",
            messages: userConversations[sessionId],
        });

        // Check if the response has the expected structure
        if (response && response.choices && response.choices.length > 0) {
            const botReply = response.choices[0].message.content;

            // Save bot's reply to the conversation history
            userConversations[sessionId].push({ role: "assistant", content: botReply });

            return res.json({ reply: botReply });
        } else {
            console.error("Unexpected response structure:", response);
            return res.status(500).json({ error: "Unexpected API response structure." });
        }
    } catch (error) {
        console.error("OpenAI API Error:", error.response ? error.response.data : error.message);

        if (error.response && error.response.status === 429) {
            return res.status(429).json({
                error: "Rate limit exceeded. Please try again later.",
            });
        }

        res.status(500).json({ error: "Chatbot error" });
    }
};
