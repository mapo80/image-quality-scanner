import { useState, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import {
  Card,
  Slider,
  Checkbox,
  Button,
  Row,
  Col,
  Image,
  Typography,
  message,
  Spin,
} from 'antd';
import axios from 'axios';

const checksList = [
  'Brisque',
  'Blur',
  'Glare',
  'Exposure',
  'Contrast',
  'ColorDominance',
  'Noise',
  'MotionBlur',
  'Banding',
];

const QualityChecker: React.FC = () => {
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [result, setResult] = useState<any>(null);
  const [loading, setLoading] = useState(false);
  const [generateHeatmaps, setGenerateHeatmaps] = useState(false);
  const [blurThreshold, setBlurThreshold] = useState(100);
  const [selectedChecks, setSelectedChecks] = useState<string[]>(checksList);

  const onDrop = useCallback((acceptedFiles: File[]) => {
    const img = acceptedFiles[0];
    if (img) {
      setFile(img);
      setPreview(URL.createObjectURL(img));
      setResult(null);
    }
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({ onDrop, accept: { 'image/*': [] } });

  const handleAnalyze = async () => {
    if (!file) {
      message.error("Carica un'immagine");
      return;
    }
    const formData = new FormData();
    formData.append('Image', file);
    selectedChecks.forEach(c => formData.append('Checks', c));
    formData.append('Settings.BlurThreshold', blurThreshold.toString());
    formData.append('Settings.GenerateHeatmaps', String(generateHeatmaps));
    try {
      setLoading(true);
      const resp = await axios.post('/quality/check', formData, { headers: { 'Content-Type': 'multipart/form-data' } });
      setResult(resp.data);
    } catch (err) {
      message.error('Errore durante il controllo');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card title="Image Quality Checker" style={{ maxWidth: 800, margin: 'auto' }}>
      <div {...getRootProps()} style={{ border: '2px dashed #aaa', padding: 20, textAlign: 'center', marginBottom: 20 }}>
        <input {...getInputProps()} />
        {isDragActive ? <p>Rilascia l\'immagine qui ...</p> : <p>Trascina un\'immagine o clicca per selezionare</p>}
      </div>

      {preview && <Image src={preview} alt="preview" style={{ marginBottom: 20 }} />}

      <Row gutter={16} style={{ marginBottom: 20 }}>
        <Col span={12}>
          <Slider
            min={0}
            max={300}
            value={blurThreshold}
            onChange={(v: number) => setBlurThreshold(v)}
            marks={{ 0: '0', 100: '100', 200: '200', 300: '300' }}
          />
          <Typography.Text>Soglia Blur: {blurThreshold}</Typography.Text>
        </Col>
        <Col span={12}>
          <Checkbox checked={generateHeatmaps} onChange={e => setGenerateHeatmaps(e.target.checked)}>Genera heatmaps</Checkbox>
        </Col>
      </Row>
      <Checkbox.Group
        options={checksList}
        value={selectedChecks}
        onChange={(vals: any) => setSelectedChecks(vals as string[])}
        style={{ marginBottom: 20 }}
      />
      <div style={{ textAlign: 'center' }}>
        <Button type="primary" onClick={handleAnalyze}>Analizza</Button>
      </div>

      {loading && <Spin style={{ display: 'block', marginTop: 20 }} />}

      {result && (
        <div style={{ marginTop: 20 }}>
          <pre style={{ background: '#f6f6f6', padding: 10 }}>{JSON.stringify(result, null, 2)}</pre>
          {generateHeatmaps && result.BlurHeatmap && (
            <Image src={`data:image/png;base64,${result.BlurHeatmap}`} alt="Blur heatmap" />
          )}
          {generateHeatmaps && result.GlareHeatmap && (
            <Image src={`data:image/png;base64,${result.GlareHeatmap}`} alt="Glare heatmap" />
          )}
        </div>
      )}
    </Card>
  );
};

export default QualityChecker;
