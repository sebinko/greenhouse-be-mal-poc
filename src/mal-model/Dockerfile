FROM python:3.9-slim

WORKDIR /app

# Copy requirements and install dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy source code and data
COPY main.py .
COPY secondary_data.csv .

# Create directory for the output model
RUN mkdir -p /app/models

# Run the training script
CMD ["python", "main.py"]