FROM python:3.10

WORKDIR /app

COPY requirements.txt .
COPY main.py .
COPY controller.py .
COPY db.py .
COPY server.py .

RUN pip install --no-cache-dir -r requirements.txt

VOLUME [ "/app/resources" ]

CMD ["python", "main.py"]
