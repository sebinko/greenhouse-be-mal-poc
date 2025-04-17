import pandas as pd
import numpy as np
import pickle
import os
from sklearn.preprocessing import LabelEncoder, StandardScaler
from sklearn.model_selection import train_test_split, GridSearchCV
from sklearn.linear_model import LogisticRegression

def load_and_preprocess_data(filepath='secondary_data.csv'):
    """Load and preprocess the mushroom dataset."""
    # Load data
    df = pd.read_csv(filepath, delimiter=';')
    
    # Drop columns with low information value
    columns_to_drop = ['veil-type', 'stem-root', 'veil-color', 'spore-print-color']
    df_cleaned = df.drop(columns=columns_to_drop)
    
    # Impute missing values with mode
    for col in df_cleaned.columns:
        if df_cleaned[col].isnull().sum() > 0:
            most_frequent = df_cleaned[col].mode()[0]
            df_cleaned[col] = df_cleaned[col].fillna(most_frequent)
    
    # Encode target variable
    label_encoder = LabelEncoder()
    y = label_encoder.fit_transform(df_cleaned['class'])
    
    # Save class mapping for later interpretation
    class_mapping = dict(zip(label_encoder.classes_, label_encoder.transform(label_encoder.classes_)))
    
    # Separate features and one-hot encode categorical variables
    X = df_cleaned.drop('class', axis=1)
    categorical_cols = X.select_dtypes(include=['object']).columns.tolist()
    X_encoded = pd.get_dummies(X, columns=categorical_cols, drop_first=True)
    
    # Scale numerical features if any exist
    numerical_cols = X_encoded.select_dtypes(include=['int64', 'float64']).columns.tolist()
    if numerical_cols:
        scaler = StandardScaler()
        X_encoded[numerical_cols] = scaler.fit_transform(X_encoded[numerical_cols])
    
    return X_encoded, y, class_mapping

def train_logistic_regression(X, y):
    """Train a logistic regression model with hyperparameter tuning."""
    # Split data into training and testing sets
    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=0.2, random_state=42, stratify=y
    )
    
    # Create and tune logistic regression model with increased max_iter
    # and different solver options
    param_grid = {
        'C': [0.01, 0.1, 1, 10, 100],
        'solver': ['liblinear', 'saga'],  # Try different solvers
        'max_iter': [5000]  # Increase max iterations
    }
    
    grid_search = GridSearchCV(
        LogisticRegression(), 
        param_grid, 
        cv=5, 
        scoring='accuracy',
        n_jobs=-1  # Use all available cores for faster training
    )
    
    print("Starting grid search with cross-validation...")
    grid_search.fit(X_train, y_train)
    
    # Get best model and parameters
    best_model = grid_search.best_estimator_
    print(f"Best parameters: {grid_search.best_params_}")
    
    return best_model, X_train.columns.tolist(), X_test, y_test

def save_model(model, columns, class_mapping, filepath=None):
    """Save the trained model and necessary metadata."""
    # Check if we're running in Docker by checking if we're in the /app directory
    if os.path.exists('/app'):
        # In Docker container, use the mounted volume path directly
        models_dir = '/app/models'
    else:
        # Local development - create models directory if it doesn't exist
        models_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', 'models')
    
    os.makedirs(models_dir, exist_ok=True)
    
    if filepath is None:
        filepath = os.path.join(models_dir, 'mushroom_classifier.pkl')
    
    model_data = {
        'model': model,
        'columns': columns,
        'class_mapping': class_mapping
    }
    
    with open(filepath, 'wb') as f:
        pickle.dump(model_data, f)
    
    print(f"Model saved to {filepath}")
    
    # Also save a timestamp file to track when the model was last updated
    with open(os.path.join(models_dir, 'last_updated.txt'), 'w') as f:
        from datetime import datetime
        f.write(f"Model last updated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")

def predict_mushroom(model_data, sample):
    """Use the trained model to predict if a mushroom is edible or poisonous."""
    # Prepare the sample in the same way as training data
    sample_df = pd.DataFrame([sample])
    
    # One-hot encode the categorical features
    # Ensure all expected columns exist
    full_columns = model_data['columns']
    sample_encoded = pd.get_dummies(sample_df)
    
    # Add missing columns with zeros
    for col in full_columns:
        if col not in sample_encoded.columns:
            sample_encoded[col] = 0
    
    # Ensure columns are in the same order as during training
    sample_encoded = sample_encoded[full_columns]
    
    # Make prediction
    prediction = model_data['model'].predict(sample_encoded)[0]
    probability = model_data['model'].predict_proba(sample_encoded)[0]
    
    # Map prediction back to original class label
    for class_label, encoded_val in model_data['class_mapping'].items():
        if encoded_val == prediction:
            predicted_class = class_label
    
    return {
        'prediction': predicted_class,
        'edible_probability': probability[1] if model_data['class_mapping']['e'] == 1 else probability[0],
        'poisonous_probability': probability[0] if model_data['class_mapping']['p'] == 0 else probability[1]
    }

if __name__ == "__main__":
    print("Starting mushroom classifier model training...")
    # Load and preprocess data
    X, y, class_mapping = load_and_preprocess_data()
    
    # Train model
    print("Training logistic regression model...")
    model, columns, X_test, y_test = train_logistic_regression(X, y)
    
    # Evaluate model
    accuracy = model.score(X_test, y_test)
    print(f"Model accuracy: {accuracy:.4f}")
    
    # Save model
    save_model(model, columns, class_mapping)
    print("Model training and saving complete!")