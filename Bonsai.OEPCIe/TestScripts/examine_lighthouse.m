%% Load raw data
fid = fopen('timestamp.csv');
x = textscan(fid,'%d -%d -%d T%d :%d :%f -%s');
time_sec_0 =  double(x{4}(1) * 3600 + x{5}(1) * 60) + x{6}(1);
ts = double(x{4}(:) .* 3600) + double(x{5}(:) .* 60) + x{6}(:) - time_sec_0;
fclose(fid);

fid = fopen('delay.raw');
delay = fread(fid, [numel(ts) 1], 'int32');
fclose(fid);

fid = fopen('type.raw');
type = fread(fid, [numel(ts) 1], 'uint8');
fclose(fid);

fid = fopen('hwtime.raw');
hw_time = fread(fid, [numel(ts) 1], 'double');
fclose(fid);

%% Cut off first few samples (seems like they are stuck in a pipe...)
n_cut = 30;
ts = ts(n_cut:end);
delay = delay(n_cut:end);
type = type(n_cut:end);
hw_time = hw_time(n_cut:end);

%% Plots
close all
plot(hw_time,'-o')

%%
plot(type)
%%
subplot(211)
plot(delay(type == 1)/10)
ylim([0 10])
title("Pulse width")

subplot(212)
plot(delay(type == 0)/10)
title("Pulse delay")



