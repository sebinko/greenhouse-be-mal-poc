from flask import Flask, request, jsonify
import pickle
import os
import pandas as pd

app = Flask(__name__)

# Load the model
def load_model():
    # Check if we're running in Docker (where the model would be in /models)
    if os.path.exists('/models'):
        model_path = '/models/mushroom_classifier.pkl'
    else:
        # Local development - try current directory first, then look for shared models directory
        local_path = os.path.join(os.path.dirname(__file__), 'mushroom_classifier.pkl')
        if os.path.exists(local_path):
            model_path = local_path
        else:
            # Try to find in the models directory (parent directory structure)
            model_path = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(__file__))), 'models', 'mushroom_classifier.pkl')
    
    print(f"Loading model from: {model_path}")
    with open(model_path, 'rb') as f:
        model_data = pickle.load(f)
    return model_data

model_data = load_model()

@app.route('/predict', methods=['POST'])
def predict():
    sample = request.json
    prediction_result = predict_mushroom(model_data, sample)
    return jsonify(prediction_result)

def predict_mushroom(model_data, sample):
    sample_df = pd.DataFrame([sample])
    sample_encoded = pd.get_dummies(sample_df)
    
    full_columns = model_data['columns']
    for col in full_columns:
        if col not in sample_encoded.columns:
            sample_encoded[col] = 0
    sample_encoded = sample_encoded[full_columns]
    
    prediction = model_data['model'].predict(sample_encoded)[0]
    probability = model_data['model'].predict_proba(sample_encoded)[0]
    
    for class_label, encoded_val in model_data['class_mapping'].items():
        if encoded_val == prediction:
            predicted_class = class_label
    
    return {
        'prediction': predicted_class,
        'edible_probability': probability[1] if model_data['class_mapping']['e'] == 1 else probability[0],
        'poisonous_probability': probability[0] if model_data['class_mapping']['p'] == 0 else probability[1]
    }

@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({"status": "healthy"})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)