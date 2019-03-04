% Exposures

exp = floor(rand(256, 256) * 4);

slope = rand(256, 256);
slope = [1.1 * slope; 1.3 * slope; 1.6 * slope; 2 * slope];
offset = 20 * rand(size(slope));

fid = fopen('exposures.raw', 'w');
fwrite(fid, exp, 'double');
fclose(fid);

fid = fopen('slope.raw', 'w');
fwrite(fid, slope, 'double');
fclose(fid);

fid = fopen('offset.raw', 'w');
fwrite(fid, offset, 'double');
fclose(fid);






